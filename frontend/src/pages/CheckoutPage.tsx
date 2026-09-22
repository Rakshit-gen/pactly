import { useEffect, useRef, useState } from 'react';
import { useMutation } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import Chip from '@mui/material/Chip';
import { Link as RouterLink } from 'react-router-dom';
import { CHECKOUT_MUTATION, SIGN_AGREEMENT_MUTATION } from '../graphql/mutations';
import { CART_QUERY, MY_ORDERS_QUERY, MY_AGREEMENTS_QUERY } from '../graphql/queries';
import { SignaturePad, type SignaturePadHandle } from '../components/SignaturePad';
import { formatCents } from '../utils/format';

interface CheckoutResult {
  order: { id: string; subtotalCents: number; prorationCreditCents: number; totalCents: number };
  agreement: { id: string; status: string; contentSnapshot: string };
}

interface CheckoutMutationData {
  checkout: CheckoutResult;
}

interface SignAgreementMutationData {
  signAgreement: { id: string; status: string };
}

export function CheckoutPage() {
  const padRef = useRef<SignaturePadHandle | null>(null);
  const [result, setResult] = useState<CheckoutResult | null>(null);
  const [signed, setSigned] = useState(false);
  const [padEmpty, setPadEmpty] = useState(true);
  const checkoutStarted = useRef(false);
  // One key per checkout attempt on this page, so a network retry or a double-fire that slips
  // past the checkoutStarted guard above replays the original order/agreement on the server
  // instead of checking out twice. A fresh page load (new attempt) gets a fresh key.
  const idempotencyKeyRef = useRef(crypto.randomUUID());

  const [checkout, { loading: checkingOut, error: checkoutError }] = useMutation<CheckoutMutationData>(
    CHECKOUT_MUTATION,
    { refetchQueries: [{ query: CART_QUERY }] },
  );
  const [signAgreement, { loading: signing, error: signError }] = useMutation<SignAgreementMutationData>(
    SIGN_AGREEMENT_MUTATION,
    { refetchQueries: [{ query: MY_ORDERS_QUERY }, { query: MY_AGREEMENTS_QUERY }] },
  );

  useEffect(() => {
    if (checkoutStarted.current) {
      return;
    }
    checkoutStarted.current = true;
    checkout({ variables: { idempotencyKey: idempotencyKeyRef.current } })
      .then(({ data }) => {
        if (data?.checkout) {
          setResult(data.checkout);
        }
      })
      .catch(() => {
        // Surfaced through checkoutError below; don't leave the rejection unhandled.
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSign = async () => {
    if (!result || !padRef.current || padRef.current.isEmpty()) {
      return;
    }
    const signatureDataUrl = padRef.current.toDataUrl();
    try {
      await signAgreement({ variables: { agreementId: result.agreement.id, signatureDataUrl } });
      setSigned(true);
    } catch {
      // Surfaced through signError; stay on the signing screen so the user can retry.
    }
  };

  if (checkingOut) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2, py: 12 }}>
        <CircularProgress />
        <Typography color="text.secondary">Drafting your agreement</Typography>
      </Box>
    );
  }

  if (checkoutError || !result) {
    return (
      <Container maxWidth="sm" sx={{ py: 10 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {checkoutError?.message ?? 'Something went wrong putting this order together.'}
        </Alert>
        <Button component={RouterLink} to="/cart" variant="outlined">
          Back to cart
        </Button>
      </Container>
    );
  }

  if (signed) {
    return (
      <Container maxWidth="sm" sx={{ py: 10 }}>
        <Chip label="Executed" sx={{ bgcolor: '#2F5D50', color: '#fff', fontWeight: 600, mb: 2 }} />
        <Typography variant="h4" sx={{ mb: 1.5 }}>
          Signed and executed
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          Your plan is active. The agreement and its audit trail are saved to your dashboard.
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button component={RouterLink} to={`/agreements/${result.agreement.id}`} variant="contained">
            View agreement
          </Button>
          <Button component={RouterLink} to="/dashboard" variant="outlined">
            Go to dashboard
          </Button>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm" sx={{ py: { xs: 6, md: 8 } }}>
      <Typography variant="h1" sx={{ fontSize: '2.2rem', mb: 4 }}>
        Review and sign
      </Typography>

      <Box sx={{ border: '1px solid', borderColor: 'divider', p: 3, mb: 3, bgcolor: 'background.paper' }}>
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ whiteSpace: 'pre-line', fontFamily: '"IBM Plex Mono", monospace', lineHeight: 1.7 }}
        >
          {result.agreement.contentSnapshot}
        </Typography>

        {result.order.prorationCreditCents > 0 && (
          <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
            <Typography variant="body2" color="text.secondary">
              Includes a {formatCents(result.order.prorationCreditCents)} credit for the unused time on your
              previous plan.
            </Typography>
          </Box>
        )}
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        This is a demo checkout, no payment is collected. Signing below executes the agreement and activates
        the plan on your account.
      </Typography>

      {signError && <Alert severity="error" sx={{ mb: 2 }}>{signError.message}</Alert>}

      <SignaturePad
        onReady={(handle) => {
          padRef.current = handle;
        }}
        onStrokeEnd={setPadEmpty}
      />

      <Button
        variant="contained"
        size="large"
        fullWidth
        sx={{ mt: 3 }}
        disabled={signing || padEmpty}
        onClick={handleSign}
      >
        {signing ? <CircularProgress size={22} color="inherit" /> : 'Sign and execute agreement'}
      </Button>
      {padEmpty && (
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
          Draw your signature above first.
        </Typography>
      )}
    </Container>
  );
}
