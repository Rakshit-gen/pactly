import { useQuery } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import { Link as RouterLink } from 'react-router-dom';
import { HeroAgreementMock } from '../components/HeroAgreementMock';
import { Reveal } from '../components/Reveal';
import { PRODUCTS_QUERY } from '../graphql/queries';
import { formatCents } from '../utils/format';
import type { Product } from '../types';

const stages = [
  {
    label: 'Draft',
    body: 'Checkout builds the agreement from your order the moment you confirm a plan or an add-on.',
  },
  {
    label: 'Sign',
    body: 'You sign on screen. The signature and timestamp are attached straight to that agreement.',
  },
  {
    label: 'Executed',
    body: 'Pactly countersigns and your entitlements update immediately. No one is waiting on email.',
  },
];

export function LandingPage() {
  const { data } = useQuery<{ products: Product[] }>(PRODUCTS_QUERY);
  const plans = (data?.products ?? []).filter((product) => product.type === 'PLAN').slice(0, 3);

  return (
    <Box>
      <Container maxWidth="lg" sx={{ pt: { xs: 8, md: 12 }, pb: { xs: 8, md: 10 } }}>
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: '1fr 420px' },
            gap: { xs: 6, md: 4 },
            alignItems: 'center',
          }}
        >
          <Box>
            <Typography variant="h1" sx={{ fontSize: { xs: '2.6rem', md: '3.4rem' }, lineHeight: 1.08, mb: 3 }}>
              Buy the plan.
              <br />
              Get the agreement.
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ fontSize: '1.15rem', maxWidth: 480, mb: 4 }}>
              Most checkouts stop at payment. Pactly turns the purchase itself into a drafted, signed, and
              executed contract, with every step logged for whoever asks later.
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Button component={RouterLink} to="/sign-up" variant="contained" color="primary" size="large" sx={{ px: 4, py: 1.3 }}>
                Start free
              </Button>
              <Button component={RouterLink} to="/pricing" variant="outlined" color="inherit" size="large" sx={{ px: 4, py: 1.3, borderColor: 'divider' }}>
                See pricing
              </Button>
            </Stack>
          </Box>

          <HeroAgreementMock />
        </Box>
      </Container>

      <Box sx={{ borderTop: '1px solid', borderColor: 'divider', py: { xs: 8, md: 10 } }}>
        <Container maxWidth="lg">
          <Typography variant="h3" sx={{ fontSize: '1.9rem', mb: 5 }}>
            What happens between clicking buy and being covered
          </Typography>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
              gap: 4,
            }}
          >
            {stages.map((stage, index) => (
              <Reveal key={stage.label} delayMs={index * 140}>
                <Box sx={{ borderTop: '2px solid', borderColor: 'primary.main', pt: 2 }}>
                  <Typography variant="caption" color="text.secondary">
                    Stage {index + 1}
                  </Typography>
                  <Typography variant="h5" sx={{ mt: 0.5, mb: 1 }}>
                    {stage.label}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {stage.body}
                  </Typography>
                </Box>
              </Reveal>
            ))}
          </Box>
        </Container>
      </Box>

      {plans.length > 0 && (
        <Box sx={{ borderTop: '1px solid', borderColor: 'divider', py: { xs: 8, md: 10 } }}>
          <Container maxWidth="lg">
            <Typography variant="h3" sx={{ fontSize: '1.9rem', mb: 1 }}>
              Plans, priced like a ledger
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 5 }}>
              One line per plan. No asterisks.
            </Typography>

            <Box sx={{ border: '1px solid', borderColor: 'divider' }}>
              {plans.map((plan, index) => (
                <Reveal key={plan.id} delayMs={index * 90}>
                  <Box
                    sx={{
                      display: 'flex',
                      flexDirection: { xs: 'column', sm: 'row' },
                      justifyContent: 'space-between',
                      alignItems: { sm: 'center' },
                      gap: 1,
                      px: 3,
                      py: 2.5,
                      borderBottom: index < plans.length - 1 ? '1px solid' : 'none',
                      borderColor: 'divider',
                    }}
                  >
                    <Box>
                      <Typography variant="h6">{plan.name}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {plan.description}
                      </Typography>
                    </Box>
                    <Typography variant="h6" sx={{ fontFamily: '"IBM Plex Mono", monospace', whiteSpace: 'nowrap' }}>
                      {formatCents(plan.monthlyPriceCents)} / mo
                    </Typography>
                  </Box>
                </Reveal>
              ))}
            </Box>

            <Button component={RouterLink} to="/pricing" variant="text" color="primary" sx={{ mt: 3 }}>
              Compare every plan and add-on
            </Button>
          </Container>
        </Box>
      )}

      <Box sx={{ bgcolor: '#14231F', color: '#ECEEE9', py: { xs: 8, md: 10 } }}>
        <Container maxWidth="lg">
          <Reveal>
            <Typography variant="h3" sx={{ fontSize: '2rem', mb: 2, color: '#ECEEE9' }}>
              Set up your first agreement in the next five minutes
            </Typography>
            <Typography variant="body1" sx={{ color: 'rgba(236,238,233,0.75)', maxWidth: 520, mb: 4 }}>
              Create an account, pick a plan, and watch checkout hand you a signed contract instead of just a
              receipt.
            </Typography>
            <Button component={RouterLink} to="/sign-up" variant="contained" size="large" sx={{ bgcolor: '#A8722E', px: 4, py: 1.3, '&:hover': { bgcolor: '#8A5B22' } }}>
              Start free
            </Button>
          </Reveal>
        </Container>
      </Box>
    </Box>
  );
}
