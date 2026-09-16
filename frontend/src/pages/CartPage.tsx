import { useMutation, useQuery } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import CircularProgress from '@mui/material/CircularProgress';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlineOutlined';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { CART_QUERY } from '../graphql/queries';
import { REMOVE_FROM_CART_MUTATION } from '../graphql/mutations';
import { formatCents } from '../utils/format';
import type { Cart } from '../types';

export function CartPage() {
  const { data, loading } = useQuery<{ cart: Cart }>(CART_QUERY, { fetchPolicy: 'cache-and-network' });
  const [removeFromCart] = useMutation(REMOVE_FROM_CART_MUTATION, {
    refetchQueries: [{ query: CART_QUERY }],
  });
  const navigate = useNavigate();

  const lines = data?.cart.lines ?? [];
  const subtotalCents = lines.reduce((total, line) => total + (line.product?.monthlyPriceCents ?? 0) * line.quantity, 0);

  if (loading && !data) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (lines.length === 0) {
    return (
      <Container maxWidth="sm" sx={{ py: 10, textAlign: 'left' }}>
        <Typography variant="h4" sx={{ mb: 1.5 }}>
          Your cart is empty
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          Pick a plan and it will show up here, ready for checkout.
        </Typography>
        <Button component={RouterLink} to="/pricing" variant="contained">
          Browse plans
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm" sx={{ py: { xs: 6, md: 8 } }}>
      <Typography variant="h1" sx={{ fontSize: '2.2rem', mb: 4 }}>
        Your cart
      </Typography>

      <Box sx={{ border: '1px solid', borderColor: 'divider', mb: 3 }}>
        {lines.map((line) => (
          <Box
            key={line.productId}
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              px: 3,
              py: 2.5,
              '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
            }}
          >
            <Box>
              <Typography variant="subtitle1">{line.product?.name ?? 'Unknown product'}</Typography>
              <Typography variant="body2" color="text.secondary">
                {line.quantity} {line.product?.type === 'PLAN' ? 'seat(s)' : 'unit(s)'}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
              <Typography variant="subtitle1" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
                {formatCents((line.product?.monthlyPriceCents ?? 0) * line.quantity)}
              </Typography>
              <IconButton
                aria-label={`Remove ${line.product?.name ?? 'item'} from cart`}
                onClick={() => removeFromCart({ variables: { productId: line.productId } })}
              >
                <DeleteOutlineIcon fontSize="small" />
              </IconButton>
            </Box>
          </Box>
        ))}
      </Box>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 4 }}>
        <Typography variant="h6">Subtotal</Typography>
        <Typography variant="h6" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
          {formatCents(subtotalCents)} / mo
        </Typography>
      </Box>

      <Button variant="contained" size="large" fullWidth onClick={() => navigate('/checkout')}>
        Proceed to checkout
      </Button>
    </Container>
  );
}
