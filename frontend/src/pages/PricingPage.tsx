import { useState } from 'react';
import { useMutation, useQuery } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { PRODUCTS_QUERY, CART_QUERY } from '../graphql/queries';
import { ADD_TO_CART_MUTATION } from '../graphql/mutations';
import { formatCents } from '../utils/format';
import type { Product } from '../types';

function PlanRow({ plan }: { plan: Product }) {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [seats, setSeats] = useState(1);
  const [addToCart, { loading }] = useMutation(ADD_TO_CART_MUTATION, {
    refetchQueries: [{ query: CART_QUERY }],
  });

  const handleAdd = async () => {
    if (!isAuthenticated) {
      navigate('/sign-up');
      return;
    }
    await addToCart({ variables: { productId: plan.id, quantity: seats } });
    navigate('/cart');
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: { xs: 'column', md: 'row' },
        justifyContent: 'space-between',
        gap: 3,
        px: { xs: 2.5, md: 3.5 },
        py: 4,
        '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
      }}
    >
      <Box sx={{ maxWidth: 420 }}>
        <Typography variant="h5">{plan.name}</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
          {plan.description}
        </Typography>
        <Box component="ul" sx={{ m: 0, pl: 2.5, color: 'text.secondary' }}>
          {plan.features.map((feature) => (
            <Typography key={feature} component="li" variant="body2">
              {feature}
            </Typography>
          ))}
        </Box>
      </Box>

      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: { xs: 'flex-start', md: 'flex-end' }, gap: 1.5, minWidth: 180 }}>
        <Typography variant="h5" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
          {formatCents(plan.monthlyPriceCents)} / mo
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            type="number"
            size="small"
            label="Seats"
            value={seats}
            onChange={(event) => setSeats(Math.max(1, Number(event.target.value) || 1))}
            slotProps={{ htmlInput: { min: 1, max: plan.seatLimit ?? undefined } }}
            sx={{ width: 90 }}
          />
          <Button variant="contained" onClick={handleAdd} disabled={loading}>
            {loading ? <CircularProgress size={20} color="inherit" /> : 'Add to cart'}
          </Button>
        </Box>
      </Box>
    </Box>
  );
}

function AddOnRow({ addOn }: { addOn: Product }) {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [addToCart, { loading }] = useMutation(ADD_TO_CART_MUTATION, {
    refetchQueries: [{ query: CART_QUERY }],
  });

  const handleAdd = async () => {
    if (!isAuthenticated) {
      navigate('/sign-up');
      return;
    }
    await addToCart({ variables: { productId: addOn.id, quantity: 1 } });
    navigate('/cart');
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: { xs: 'column', sm: 'row' },
        justifyContent: 'space-between',
        alignItems: { sm: 'center' },
        gap: 2,
        px: { xs: 2.5, md: 3.5 },
        py: 2.5,
        '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
      }}
    >
      <Box>
        <Typography variant="subtitle1">{addOn.name}</Typography>
        <Typography variant="body2" color="text.secondary">
          {addOn.description}
        </Typography>
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Typography variant="subtitle1" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
          {formatCents(addOn.monthlyPriceCents)} / mo
        </Typography>
        <Button variant="outlined" onClick={handleAdd} disabled={loading}>
          {loading ? <CircularProgress size={20} /> : 'Add'}
        </Button>
      </Box>
    </Box>
  );
}

export function PricingPage() {
  const { data, loading, error } = useQuery<{ products: Product[] }>(PRODUCTS_QUERY);

  const plans = (data?.products ?? []).filter((p) => p.type === 'PLAN');
  const addOns = (data?.products ?? []).filter((p) => p.type === 'ADD_ON');

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 6, md: 8 } }}>
      <Typography variant="h1" sx={{ fontSize: { xs: '2.2rem', md: '2.8rem' }, mb: 1 }}>
        Pricing
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 5, maxWidth: 560 }}>
        Every plan includes the full agreement lifecycle. Add-ons stack on top of whatever plan you are on.
      </Typography>

      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      )}

      {error && <Alert severity="error">Could not load pricing right now. Try refreshing the page.</Alert>}

      {plans.length > 0 && (
        <Box sx={{ border: '1px solid', borderColor: 'divider', mb: 6 }}>
          {plans.map((plan) => (
            <PlanRow key={plan.id} plan={plan} />
          ))}
        </Box>
      )}

      {addOns.length > 0 && (
        <>
          <Typography variant="h5" sx={{ mb: 2 }}>
            Add-ons
          </Typography>
          <Box sx={{ border: '1px solid', borderColor: 'divider' }}>
            {addOns.map((addOn) => (
              <AddOnRow key={addOn.id} addOn={addOn} />
            ))}
          </Box>
        </>
      )}
    </Container>
  );
}
