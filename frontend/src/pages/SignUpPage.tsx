import { useState, type FormEvent } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import { AuthCard } from '../components/AuthCard';
import { useAuth } from '../auth/AuthContext';

export function SignUpPage() {
  const { register, loading } = useAuth();
  const navigate = useNavigate();
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    try {
      await register(email, password, displayName);
      navigate('/pricing');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Could not create your account.';
      setError(message);
    }
  };

  return (
    <AuthCard title="Create an account" subtitle="Takes about thirty seconds.">
      <Stack component="form" spacing={2.5} onSubmit={handleSubmit}>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField
          label="Name"
          value={displayName}
          onChange={(event) => setDisplayName(event.target.value)}
          required
          fullWidth
        />
        <TextField
          label="Email"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
          fullWidth
        />
        <TextField
          label="Password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
          fullWidth
          helperText="At least 8 characters."
        />
        <Button type="submit" variant="contained" size="large" disabled={loading}>
          {loading ? <CircularProgress size={22} color="inherit" /> : 'Create account'}
        </Button>
        <Typography variant="body2" color="text.secondary">
          Already have an account? <RouterLink to="/log-in">Log in</RouterLink>
        </Typography>
      </Stack>
    </AuthCard>
  );
}
