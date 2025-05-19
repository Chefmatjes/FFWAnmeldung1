import React from 'react';
import { Box, Typography, Paper, Divider, Button } from '@mui/material';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';

const SuccessScreen = ({ responseData }) => {
  return (
    <Box sx={{ textAlign: 'center', py: 2 }}>
      <CheckCircleOutlineIcon sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
      
      <Typography variant="h5" gutterBottom>
        Anmeldung erfolgreich übermittelt!
      </Typography>
      
      <Typography variant="body1" paragraph>
        Vielen Dank für deine Anmeldung bei der Freiwilligen Feuerwehr Apfeltrang.
      </Typography>
      
      <Paper elevation={1} sx={{ p: 3, mb: 3, textAlign: 'left', maxWidth: '600px', mx: 'auto' }}>
        <Typography variant="subtitle1" gutterBottom>
          Übermittlungsdetails:
        </Typography>
        
        <Divider sx={{ my: 1 }} />
        
        <Box sx={{ mt: 2 }}>
          <Typography variant="body2">
            <strong>Dokument:</strong> {responseData?.fileName}
          </Typography>
          
          <Typography variant="body2" sx={{ mt: 1 }}>
            <strong>Gesendet an:</strong> vorstand@ff-apfeltrang.de
          </Typography>
        </Box>
      </Paper>
      
      <Typography variant="body2" color="text.secondary" paragraph>
        Wir werden Ihre Anmeldung bearbeiten und uns bei Ihnen melden.
      </Typography>
      
      <Button 
        variant="contained" 
        color="primary"
        onClick={() => window.location.reload()}
      >
        Neue Anmeldung erstellen
      </Button>
    </Box>
  );
};

export default SuccessScreen; 