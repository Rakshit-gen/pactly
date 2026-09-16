import { useQuery } from '@apollo/client/react';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Badge from '@mui/material/Badge';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined';
import { useState, type MouseEvent } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { CART_QUERY } from '../graphql/queries';
import type { Cart } from '../types';

export function Navbar() {
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const { data } = useQuery<{ cart: Cart }>(CART_QUERY, { skip: !isAuthenticated });
  const itemCount = data?.cart.lines.reduce((total, line) => total + line.quantity, 0) ?? 0;

  const openMenu = (event: MouseEvent<HTMLElement>) => setAnchorEl(event.currentTarget);
  const closeMenu = () => setAnchorEl(null);

  const handleLogout = () => {
    closeMenu();
    logout();
    navigate('/');
  };

  return (
    <AppBar
      position="sticky"
      elevation={0}
      color="transparent"
      sx={{
        borderBottom: '1px solid',
        borderColor: 'divider',
        backgroundColor: 'background.default',
        backdropFilter: 'blur(6px)',
      }}
    >
      <Toolbar sx={{ maxWidth: 1160, width: '100%', mx: 'auto', px: { xs: 2, sm: 3 } }}>
        <Typography
          component={RouterLink}
          to="/"
          variant="h6"
          sx={{ flexGrow: 1, textDecoration: 'none', color: 'text.primary', fontFamily: '"Newsreader", serif', fontSize: '1.4rem' }}
        >
          Pactly
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button component={RouterLink} to="/pricing" color="inherit">
            Pricing
          </Button>

          {isAuthenticated ? (
            <>
              <IconButton component={RouterLink} to="/cart" aria-label="View cart">
                <Badge badgeContent={itemCount} color="primary">
                  <ShoppingBagOutlinedIcon />
                </Badge>
              </IconButton>
              <Button color="inherit" onClick={openMenu}>
                {user?.displayName ?? 'Account'}
              </Button>
              <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={closeMenu}>
                <MenuItem component={RouterLink} to="/dashboard" onClick={closeMenu}>
                  Dashboard
                </MenuItem>
                <MenuItem onClick={handleLogout}>Log out</MenuItem>
              </Menu>
            </>
          ) : (
            <>
              <Button component={RouterLink} to="/log-in" color="inherit">
                Log in
              </Button>
              <Button component={RouterLink} to="/sign-up" variant="contained" color="primary">
                Get started
              </Button>
            </>
          )}
        </Box>
      </Toolbar>
    </AppBar>
  );
}
