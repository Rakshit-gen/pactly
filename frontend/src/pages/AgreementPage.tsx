import { useParams } from 'react-router-dom';
import { useQuery } from '@apollo/client/react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import { AGREEMENT_QUERY } from '../graphql/queries';
import { formatDate } from '../utils/format';
import type { Agreement } from '../types';

const statusColors: Record<string, string> = {
  DRAFT: '#8A8578',
  AWAITING_SIGNATURE: '#A8722E',
  SIGNED: '#2F5D50',
  EXECUTED: '#2F5D50',
};

export function AgreementPage() {
  const { id } = useParams<{ id: string }>();
  const { data, loading, error } = useQuery<{ agreement: Agreement | null }>(AGREEMENT_QUERY, {
    variables: { id },
    skip: !id,
  });

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data?.agreement) {
    return (
      <Container maxWidth="sm" sx={{ py: 10 }}>
        <Alert severity="error">This agreement could not be found.</Alert>
      </Container>
    );
  }

  const agreement = data.agreement;

  return (
    <Container maxWidth="sm" sx={{ py: { xs: 6, md: 8 } }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 3 }}>
        <Typography variant="h1" sx={{ fontSize: '2rem' }}>
          Agreement {agreement.id.slice(-6)}
        </Typography>
        <Chip
          label={agreement.status.replace('_', ' ').toLowerCase()}
          size="small"
          sx={{ bgcolor: statusColors[agreement.status] ?? '#8A8578', color: '#fff', textTransform: 'capitalize' }}
        />
      </Box>

      <Box sx={{ border: '1px solid', borderColor: 'divider', p: 3, mb: 4, bgcolor: 'background.paper' }}>
        <Typography
          variant="body2"
          sx={{ whiteSpace: 'pre-line', fontFamily: '"IBM Plex Mono", monospace', lineHeight: 1.7 }}
        >
          {agreement.contentSnapshot}
        </Typography>

        {agreement.signatureDataUrl && (
          <Box sx={{ mt: 3, pt: 3, borderTop: '1px solid', borderColor: 'divider' }}>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
              Signature
            </Typography>
            <Box
              component="img"
              src={agreement.signatureDataUrl}
              alt="Signature"
              sx={{ maxWidth: 220, display: 'block' }}
            />
          </Box>
        )}
      </Box>

      <Typography variant="h6" sx={{ mb: 2 }}>
        Audit trail
      </Typography>
      <Box sx={{ border: '1px solid', borderColor: 'divider' }}>
        {(agreement.auditTrail ?? []).map((entry, index) => (
          <Box
            key={`${entry.action}-${index}`}
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start',
              gap: 2,
              px: 3,
              py: 2,
              '&:not(:last-of-type)': { borderBottom: '1px solid', borderColor: 'divider' },
            }}
          >
            <Box>
              <Typography variant="body2">{entry.action}</Typography>
              <Typography variant="caption" color="text.secondary">
                {entry.actor}, {entry.metadata}
              </Typography>
            </Box>
            <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
              {formatDate(entry.timestamp)}
            </Typography>
          </Box>
        ))}
      </Box>
    </Container>
  );
}
