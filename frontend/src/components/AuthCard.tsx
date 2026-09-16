import type { ReactNode } from 'react';
import Container from '@mui/material/Container';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

export function AuthCard({ title, subtitle, children }: { title: string; subtitle: string; children: ReactNode }) {
  return (
    <Container maxWidth="xs" sx={{ py: { xs: 8, md: 12 } }}>
      <Typography variant="h3" sx={{ fontSize: '2rem', mb: 0.5 }}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
        {subtitle}
      </Typography>
      <Box sx={{ border: '1px solid', borderColor: 'divider', p: 3.5, bgcolor: 'background.paper' }}>{children}</Box>
    </Container>
  );
}
