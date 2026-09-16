import { useRef } from 'react';
import SignatureCanvas from 'react-signature-canvas';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';

export interface SignaturePadHandle {
  isEmpty: () => boolean;
  toDataUrl: () => string;
  clear: () => void;
}

export function SignaturePad({
  onReady,
  onStrokeEnd,
}: {
  onReady?: (handle: SignaturePadHandle) => void;
  onStrokeEnd?: (isEmpty: boolean) => void;
}) {
  const canvasRef = useRef<SignatureCanvas>(null);

  const handleRef = (instance: SignatureCanvas | null) => {
    canvasRef.current = instance;
    if (instance && onReady) {
      onReady({
        isEmpty: () => instance.isEmpty(),
        toDataUrl: () => instance.getCanvas().toDataURL('image/png'),
        clear: () => instance.clear(),
      });
    }
  };

  return (
    <Box>
      <Box
        sx={{
          border: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
          position: 'relative',
        }}
      >
        <SignatureCanvas
          ref={handleRef}
          penColor="#14231F"
          onEnd={() => onStrokeEnd?.(canvasRef.current?.isEmpty() ?? true)}
          canvasProps={{ width: 460, height: 160, style: { width: '100%', height: 160, display: 'block' } }}
        />
        <Box
          sx={{
            position: 'absolute',
            left: 24,
            right: 24,
            bottom: 28,
            borderBottom: '1px solid',
            borderColor: 'text.secondary',
            pointerEvents: 'none',
          }}
        />
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1 }}>
        <Typography variant="caption" color="text.secondary">
          Sign above the line
        </Typography>
        <Button
          size="small"
          onClick={() => {
            canvasRef.current?.clear();
            onStrokeEnd?.(true);
          }}
        >
          Clear
        </Button>
      </Box>
    </Box>
  );
}
