import React, { useState } from 'react';
import { Box, Container, Paper, Typography, Stepper, Step, StepLabel } from '@mui/material';
import RegistrationForm from './components/RegistrationForm';
import SuccessScreen from './components/SuccessScreen';
import axios from 'axios';

const steps = ['Antragsformular', 'Bestätigung'];

function App() {
  const [activeStep, setActiveStep] = useState(0);
  const [responseData, setResponseData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleNext = () => {
    setActiveStep((prevActiveStep) => prevActiveStep + 1);
  };

  const handleFormSubmit = async (formData) => {
    setLoading(true);
    setError(null);

    try {
      console.log('Submitting form data:', formData);
      
      // 1. Submit form data to generate PDF
      const response = await axios.post('/api/form/submit', formData);
      
      console.log('Form submit response:', response.data);
      
      if (!response.data || !response.data.success) {
        throw new Error('PDF-Generierung fehlgeschlagen');
      }

      const { filePath, fileName } = response.data;
      
      // 2. Send the PDF via email to fixed address
      const deliveryResponse = await axios.post('/api/email/send', {
        filePath: filePath,
        // No custom email address, will use default from server config
      });
      
      console.log('Delivery response:', deliveryResponse.data);
      
      if (!deliveryResponse.data || !deliveryResponse.data.success) {
        throw new Error('Übermittlung per E-Mail fehlgeschlagen');
      }

      // Set response data and move to next step
      setResponseData({
        method: 'email',
        fileName,
        recipientEmail: 'vorstand@ff-apfeltrang.de', // Fixed recipient
        ...deliveryResponse.data
      });
      
      handleNext();
    } catch (err) {
      console.error('Error submitting form:', err);
      setError(err.message || 'Ein Fehler ist aufgetreten. Bitte versuchen Sie es später erneut.');
    } finally {
      setLoading(false);
    }
  };

  const getStepContent = (step) => {
    switch (step) {
      case 0:
        return <RegistrationForm onSubmit={handleFormSubmit} loading={loading} error={error} />;
      case 1:
        return <SuccessScreen responseData={responseData} />;
      default:
        return 'Unbekannter Schritt';
    }
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Stepper activeStep={activeStep}>
            {steps.map((label) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
              </Step>
            ))}
          </Stepper>
        </Box>
        {getStepContent(activeStep)}
      </Paper>
    </Container>
  );
}

export default App; 