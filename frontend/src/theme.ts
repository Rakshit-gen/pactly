import { createTheme } from '@mui/material/styles';

const ink = '#14231F';
const inkSoft = '#4B564F';
const paper = '#ECEEE9';
const paperCard = '#F7F7F2';
const seal = '#A8722E';
const sealDark = '#8A5B22';
const ledger = '#2F5D50';
const line = '#D8D6CB';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: seal,
      dark: sealDark,
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: ledger,
      contrastText: '#FFFFFF',
    },
    background: {
      default: paper,
      paper: paperCard,
    },
    text: {
      primary: ink,
      secondary: inkSoft,
    },
    divider: line,
  },
  shape: {
    borderRadius: 2,
  },
  typography: {
    fontFamily: '"IBM Plex Sans", "Helvetica Neue", Arial, sans-serif',
    h1: { fontFamily: '"Newsreader", serif', fontWeight: 500, letterSpacing: '-0.01em' },
    h2: { fontFamily: '"Newsreader", serif', fontWeight: 500, letterSpacing: '-0.01em' },
    h3: { fontFamily: '"Newsreader", serif', fontWeight: 500 },
    h4: { fontFamily: '"Newsreader", serif', fontWeight: 500 },
    h5: { fontFamily: '"Newsreader", serif', fontWeight: 500 },
    h6: { fontFamily: '"IBM Plex Sans", sans-serif', fontWeight: 600 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 2,
          boxShadow: 'none',
        },
        contained: {
          boxShadow: 'none',
          '&:hover': { boxShadow: 'none' },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: `1px solid ${line}`,
          boxShadow: 'none',
        },
      },
    },
  },
});

export default theme;
