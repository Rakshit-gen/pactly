import { useQuery } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link as RouterLink } from 'react-router-dom';
import { ME_QUERY, MY_ORDERS_QUERY, MY_AGREEMENTS_QUERY } from '../graphql/queries';
import { formatCents, formatDate } from '../utils/format';
import type { Agreement, AgreementStatus, AuthUser, Order } from '../types';

const statusColors: Record<AgreementStatus, string> = {
  DRAFT: '#8A8578',
  AWAITING_SIGNATURE: '#A8722E',
  SIGNED: '#2F5D50',
  EXECUTED: '#2F5D50',
};

export function DashboardPage() {
  const { data: meData } = useQuery<{ me: AuthUser & { currentPlanId: string | null; currentSeats: number; currentPeriodEnd: string | null } }>(
    ME_QUERY,
  );
  const { data: ordersData, loading: ordersLoading } = useQuery<{ myOrders: Order[] }>(MY_ORDERS_QUERY);
  const { data: agreementsData, loading: agreementsLoading } = useQuery<{ myAgreements: Agreement[] }>(MY_AGREEMENTS_QUERY);

  const me = meData?.me;

  return (
    <Container maxWidth="md" sx={{ py: { xs: 6, md: 8 } }}>
      <Typography variant="h1" sx={{ fontSize: '2.2rem', mb: 4 }}>
        {me ? `Welcome back, ${me.displayName.split(' ')[0]}` : 'Dashboard'}
      </Typography>

      <Box sx={{ border: '1px solid', borderColor: 'divider', p: 3, mb: 5, display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="overline" color="text.secondary">
            Current plan
          </Typography>
          <Typography variant="h5">
            {me?.currentPlanId ? `${me.currentSeats} seat(s) active` : 'No active plan yet'}
          </Typography>
        </Box>
        <Button component={RouterLink} to="/pricing" variant="outlined">
          {me?.currentPlanId ? 'Change plan' : 'Browse plans'}
        </Button>
      </Box>

      <Typography variant="h5" sx={{ mb: 2 }}>
        Orders
      </Typography>
      {ordersLoading && <CircularProgress size={24} />}
      {!ordersLoading && (ordersData?.myOrders.length ?? 0) === 0 && (
        <Typography color="text.secondary" sx={{ mb: 5 }}>
          No orders yet.
        </Typography>
      )}
      {(ordersData?.myOrders.length ?? 0) > 0 && (
        <Box sx={{ border: '1px solid', borderColor: 'divider', mb: 5 }}>
          {ordersData!.myOrders.map((order) => (
            <Box
              key={order.id}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                px: 3,
                py: 2,
                '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
              }}
            >
              <Box>
                <Typography variant="body1">{order.lines.map((l) => l.productName).join(', ')}</Typography>
                <Typography variant="caption" color="text.secondary">
                  Placed {formatDate(order.createdAt)}, {order.status.toLowerCase()}
                </Typography>
              </Box>
              <Typography variant="body1" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
                {formatCents(order.totalCents)}
              </Typography>
            </Box>
          ))}
        </Box>
      )}

      <Typography variant="h5" sx={{ mb: 2 }}>
        Agreements
      </Typography>
      {agreementsLoading && <CircularProgress size={24} />}
      {!agreementsLoading && (agreementsData?.myAgreements.length ?? 0) === 0 && (
        <Typography color="text.secondary">No agreements yet.</Typography>
      )}
      {(agreementsData?.myAgreements.length ?? 0) > 0 && (
        <Box sx={{ border: '1px solid', borderColor: 'divider' }}>
          {agreementsData!.myAgreements.map((agreement) => (
            <Box
              key={agreement.id}
              component={RouterLink}
              to={`/agreements/${agreement.id}`}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                px: 3,
                py: 2,
                textDecoration: 'none',
                color: 'inherit',
                '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
                '&:hover': { bgcolor: 'background.paper' },
              }}
            >
              <Typography variant="body1">Agreement {agreement.id.slice(-6)}</Typography>
              <Chip
                label={agreement.status.replace('_', ' ').toLowerCase()}
                size="small"
                sx={{ bgcolor: statusColors[agreement.status], color: '#fff', textTransform: 'capitalize' }}
              />
            </Box>
          ))}
        </Box>
      )}
    </Container>
  );
}
