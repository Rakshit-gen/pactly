import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import { keyframes } from '@emotion/react';

const draw = keyframes`
  from { stroke-dashoffset: 420; }
  to { stroke-dashoffset: 0; }
`;

export function HeroAgreementMock() {
  return (
    <Box
      sx={{
        position: 'relative',
        width: '100%',
        maxWidth: 380,
        mx: { xs: 'auto', md: 0 },
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          transform: 'rotate(2.5deg)',
          bgcolor: '#E4E1D4',
          border: '1px solid',
          borderColor: 'divider',
        }}
        aria-hidden
      />
      <Box
        sx={{
          position: 'relative',
          bgcolor: 'background.paper',
          border: '1px solid',
          borderColor: 'divider',
          p: 3.5,
          transform: 'rotate(-1deg)',
        }}
      >
        <Typography
          variant="overline"
          sx={{ letterSpacing: 0, fontFamily: '"IBM Plex Sans", sans-serif', color: 'text.secondary', fontSize: '0.75rem' }}
        >
          Agreement 4471
        </Typography>
        <Typography variant="h5" sx={{ mt: 0.5, mb: 2 }}>
          Growth plan
        </Typography>

        <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 1, borderTop: '1px solid', borderColor: 'divider' }}>
          <Typography variant="body2" color="text.secondary">
            Growth plan &times; 7 seats
          </Typography>
          <Typography variant="body2" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
            $413.00/mo
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 1, borderTop: '1px solid', borderColor: 'divider' }}>
          <Typography variant="body2" color="text.secondary">
            Priority support
          </Typography>
          <Typography variant="body2" sx={{ fontFamily: '"IBM Plex Mono", monospace' }}>
            $15.00/mo
          </Typography>
        </Box>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            py: 1.25,
            borderTop: '1px solid',
            borderColor: 'text.primary',
            mb: 2,
          }}
        >
          <Typography variant="body2" sx={{ fontWeight: 600 }}>
            Due today
          </Typography>
          <Typography variant="body2" sx={{ fontWeight: 600, fontFamily: '"IBM Plex Mono", monospace' }}>
            $428.00
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
              Signed by J. Alvarez, executed by Pactly
            </Typography>
            <Box component="svg" viewBox="0 0 180 55" sx={{ width: 130, height: 40, mt: 0.5 }} aria-hidden>
              <Box
                component="path"
                d="M8,38 C 18,10 28,55 42,26 C 52,6 62,46 78,22 C 90,4 104,42 122,18 C 134,4 142,32 156,14"
                fill="none"
                stroke="#14231F"
                strokeWidth={2.2}
                strokeLinecap="round"
                strokeDasharray={420}
                sx={{
                  animation: `${draw} 1.4s ease-out 0.3s both`,
                  '@media (prefers-reduced-motion: reduce)': {
                    animation: 'none',
                    strokeDashoffset: 0,
                  },
                }}
              />
            </Box>
          </Box>
          <Chip label="Executed" size="small" sx={{ bgcolor: '#2F5D50', color: '#fff', fontWeight: 600 }} />
        </Box>
      </Box>
    </Box>
  );
}
