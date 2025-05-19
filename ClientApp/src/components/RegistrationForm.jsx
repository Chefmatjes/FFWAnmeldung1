import React, { useRef, useState, useEffect } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { 
  Box, 
  Button, 
  TextField, 
  Grid, 
  MenuItem, 
  Typography,
  FormControlLabel,
  Checkbox,
  Divider,
  Paper,
  Radio,
  RadioGroup,
  CircularProgress,
  Alert
} from '@mui/material';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs from 'dayjs';
import 'dayjs/locale/de';
import SignatureCanvas from 'react-signature-canvas';

// Signature field component
const SignatureField = ({ label, value, onChange, error, helperText }) => {
  const sigCanvasRef = useRef(null);
  const [isEmpty, setIsEmpty] = useState(true);
  
  // Fallback to text field if SignatureCanvas is not available
  // Remove this check when the package is properly installed
  const SignatureCanvasFallback = ({ ref, ...props }) => (
    <div 
      ref={ref}
      style={{ 
        width: '100%', 
        height: 120, 
        border: '1px solid #ccc',
        borderRadius: '4px',
        position: 'relative',
        marginBottom: '8px',
        backgroundColor: '#f8f8f8'
      }}
      {...props}
    >
      <div style={{ 
        position: 'absolute', 
        top: '50%', 
        left: '50%', 
        transform: 'translate(-50%, -50%)',
        color: '#999',
        fontStyle: 'italic'
      }}>
        Signaturfeld (Hier klicken und zeichnen)
      </div>
    </div>
  );
  
  // Use the actual component or fallback
  const SignatureComponent = typeof SignatureCanvas !== 'undefined' 
    ? SignatureCanvas 
    : SignatureCanvasFallback;
  
  const handleClear = () => {
    if (sigCanvasRef.current && sigCanvasRef.current.clear) {
      sigCanvasRef.current.clear();
      setIsEmpty(true);
      onChange('');
    }
  };
  
  const handleEnd = () => {
    if (sigCanvasRef.current) {
      if (sigCanvasRef.current.isEmpty && sigCanvasRef.current.isEmpty()) {
        setIsEmpty(true);
        onChange('');
      } else {
        setIsEmpty(false);
        // When using the actual SignatureCanvas, this will get the data URL
        if (sigCanvasRef.current.toDataURL) {
          const signatureData = sigCanvasRef.current.toDataURL();
          onChange(signatureData);
        } else {
          // Fallback for our mock component
          onChange('signature-data-placeholder');
        }
      }
    }
  };
  
  // This effect ensures the canvas size is set correctly
  useEffect(() => {
    if (sigCanvasRef.current && sigCanvasRef.current.getCanvas) {
      const canvas = sigCanvasRef.current.getCanvas();
      if (canvas) {
        // Force a resize to set the internal canvas dimensions correctly
        const parentWidth = canvas.parentElement.clientWidth;
        canvas.width = parentWidth;
        canvas.height = 120;
        
        // If we have previous data, redraw it
        if (value && sigCanvasRef.current.fromDataURL) {
          sigCanvasRef.current.fromDataURL(value);
        }
      }
    }
  }, [value]);
  
  return (
    <div>
      <Typography variant="body1" gutterBottom>{label}</Typography>
      <div style={{ width: '100%', position: 'relative' }}>
        <SignatureComponent
          ref={sigCanvasRef}
          penColor="black"
          canvasProps={{
            className: error ? 'signature-pad-error' : 'signature-pad',
            style: { 
              width: '100%', 
              height: '120px'
            }
          }}
          onEnd={handleEnd}
        />
      </div>
      {error && <Typography color="error" variant="caption">{helperText}</Typography>}
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '4px' }}>
        <Button 
          size="small" 
          variant="outlined" 
          onClick={handleClear}
          disabled={isEmpty}
        >
          Löschen
        </Button>
      </div>
      
      {/* Note: style jsx requires the styled-jsx package */}
      <style dangerouslySetInnerHTML={{ __html: `
        .signature-pad {
          border: 1px solid #ccc;
          border-radius: 4px;
        }
        .signature-pad-error {
          border: 1px solid #d32f2f;
          border-radius: 4px;
        }
      `}} />
    </div>
  );
};

const validationSchema = Yup.object({
  firstName: Yup.string().required('Vorname ist erforderlich'),
  lastName: Yup.string().required('Nachname ist erforderlich'),
  birthDate: Yup.date().required('Geburtsdatum ist erforderlich'),
  street: Yup.string().required('Straße ist erforderlich'),
  postalCode: Yup.string().matches(/^\d{5}$/, 'Postleitzahl muss 5-stellig sein').required('Postleitzahl ist erforderlich'),
  city: Yup.string().required('Ort ist erforderlich'),
  phone: Yup.string().required('Telefonnummer ist erforderlich'),
  mobile: Yup.string(),
  email: Yup.string().email('Ungültige E-Mail-Adresse').required('E-Mail ist erforderlich'),
  whatsappGroup: Yup.string().required('Bitte wählen Sie Ja oder Nein'),
  previousFireDepartment: Yup.string(),
  entryDate: Yup.date().nullable(),
  activeMember: Yup.string().required('Bitte wählen Sie Ja oder Nein'),
  accountHolder: Yup.string(),
  bic: Yup.string(),
  iban: Yup.string(),
  place: Yup.string().required('Ort ist erforderlich'),
  signatureDate: Yup.date().required('Datum ist erforderlich'),
  signature: Yup.string().required('Unterschrift ist erforderlich'),
  parentSignature: Yup.string()
});

const departments = [
  { value: 'Einsatzabteilung', label: 'Einsatzabteilung' },
  { value: 'Jugendfeuerwehr', label: 'Jugendfeuerwehr' },
  { value: 'Kinderfeuerwehr', label: 'Kinderfeuerwehr' },
  { value: 'Alters- und Ehrenabteilung', label: 'Alters- und Ehrenabteilung' },
];

const membershipTypes = [
  { value: 'Aktives Mitglied', label: 'Aktives Mitglied' },
  { value: 'Förderndes Mitglied', label: 'Förderndes Mitglied' },
];

const RegistrationForm = ({ onSubmit, loading, error }) => {
  const formik = useFormik({
    initialValues: {
      firstName: '',
      lastName: '',
      birthDate: null,
      street: '',
      postalCode: '',
      city: '',
      phone: '',
      mobile: '',
      email: '',
      whatsappGroup: 'nein',
      activeMember: 'nein',
      previousFireDepartment: '',
      entryDate: null,
      accountHolder: '',
      bic: '',
      iban: '',
      place: '',
      signatureDate: dayjs(),
      signature: '',
      parentSignature: ''
    },
    validationSchema,
    onSubmit: (values) => {
      // Formatiere die Werte für die API
      const apiValues = {
        ...values,
        department: values.activeMember === 'ja' ? 'Aktive Abteilung' : 'Förderndes Mitglied',
        membershipType: values.activeMember === 'ja' ? 'Aktives Mitglied' : 'Förderndes Mitglied',
        whatsappGroup: values.whatsappGroup === 'ja',
        activeMember: values.activeMember === 'ja'
      };
      onSubmit(apiValues);
    },
  });

  // Debug-Funktion zum Anzeigen von Validierungsfehlern
  const handleDebugSubmit = () => {
    // Validiere das Formular
    formik.validateForm().then(errors => {
      console.log('Validierungsfehler:', errors);
      if (Object.keys(errors).length === 0) {
        console.log('Formular ist valid. Alle Felder sind korrekt ausgefüllt.');
      } else {
        console.log('Diese Felder sind fehlerhaft:');
        Object.entries(errors).forEach(([field, errorMsg]) => {
          console.log(`- ${field}: ${errorMsg}`);
        });
        
        // Zeige auch die aktuellen Formularwerte
        console.log('Aktuelle Formularwerte:', formik.values);
      }
    });
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="de">
      <form onSubmit={formik.handleSubmit}>
        {/* Centered Logo and Header */}
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 2 }}>
          <img src="/images/logo.png" alt="Feuerwehr Logo" style={{ width: 180, marginBottom: 16 }} />
          <Typography variant="h4" fontWeight="bold" align="center">
            Freiwillige Feuerwehr Apfeltrang e.V.
          </Typography>
        </Box>
        {/* Info Box (right) */}
        <Box sx={{ display: 'flex', justifyContent: 'center', mb: 4 }}>
          <Paper elevation={1} sx={{ p: 2, maxWidth: 350 }}>
            <Typography variant="body2" fontWeight="bold">
              1. Vorsitzender:
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              Michael Stich
            </Typography>
            <Typography variant="body2">
              Im Obstgarten 29
            </Typography>
            <Typography variant="body2" sx={{ mb: 1 }}>
              87674 Ruderatshofen
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              1. Kommandant:
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              Pascal Reimann
            </Typography>
            <Typography variant="body2">
              Apfeltranger Dorfstr. 15
            </Typography>
            <Typography variant="body2" sx={{ mb: 1 }}>
              87674 Ruderatshofen
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              Mail:
            </Typography>
            <Typography variant="body2">
              Vorstand@ff-apfeltrang.de
            </Typography>
          </Paper>
        </Box>

        {/* Haupttitel */}
        <Typography variant="h4" sx={{ mb: 3, textAlign: 'center' }}>
          Beitrittserklärung
        </Typography>

        <Typography variant="body1" fontWeight="bold" sx={{ mb: 2 }}>
          hiermit erkläre ich,
        </Typography>

        {/* Persönliche Daten */}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
            {formik.submitCount > 0 && Object.keys(formik.errors).length > 0 && (
              <ul style={{ margin: 0, paddingLeft: 20 }}>
                {Object.entries(formik.errors).map(([field, msg]) => (
                  <li key={field}>{msg}</li>
                ))}
              </ul>
            )}
          </Alert>
        )}
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="firstName"
              name="firstName"
              label="Vorname"
              variant="standard"
              value={formik.values.firstName || ''}
              onChange={formik.handleChange}
              error={formik.touched.firstName && Boolean(formik.errors.firstName)}
              helperText={formik.touched.firstName && formik.errors.firstName}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="lastName"
              name="lastName"
              label="Nachname"
              variant="standard"
              value={formik.values.lastName || ''}
              onChange={formik.handleChange}
              error={formik.touched.lastName && Boolean(formik.errors.lastName)}
              helperText={formik.touched.lastName && formik.errors.lastName}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="street"
              name="street"
              label="Straße"
              variant="standard"
              value={formik.values.street || ''}
              onChange={formik.handleChange}
              error={formik.touched.street && Boolean(formik.errors.street)}
              helperText={formik.touched.street && formik.errors.street}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={2}>
            <TextField
              fullWidth
              id="postalCode"
              name="postalCode"
              label="PLZ"
              variant="standard"
              value={formik.values.postalCode || ''}
              onChange={formik.handleChange}
              error={formik.touched.postalCode && Boolean(formik.errors.postalCode)}
              helperText={formik.touched.postalCode && formik.errors.postalCode}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <TextField
              fullWidth
              id="city"
              name="city"
              label="Ort"
              variant="standard"
              value={formik.values.city || ''}
              onChange={formik.handleChange}
              error={formik.touched.city && Boolean(formik.errors.city)}
              helperText={formik.touched.city && formik.errors.city}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <DatePicker
              label="Geburtsdatum"
              value={formik.values.birthDate}
              onChange={(value) => formik.setFieldValue('birthDate', value)}
              slotProps={{
                textField: {
                  fullWidth: true,
                  variant: 'standard',
                  error: formik.touched.birthDate && Boolean(formik.errors.birthDate),
                  helperText: formik.touched.birthDate && formik.errors.birthDate,
                  required: true,
                  sx: {}
                },
              }}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <TextField
              fullWidth
              id="phone"
              name="phone"
              label="Telefon"
              variant="standard"
              value={formik.values.phone || ''}
              onChange={formik.handleChange}
              error={formik.touched.phone && Boolean(formik.errors.phone)}
              helperText={formik.touched.phone && formik.errors.phone}
              sx={{}}
              required
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <TextField
              fullWidth
              id="mobile"
              name="mobile"
              label="Mobil"
              variant="standard"
              value={formik.values.mobile || ''}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <TextField
              fullWidth
              id="email"
              name="email"
              label="E-Mail"
              variant="standard"
              value={formik.values.email || ''}
              onChange={formik.handleChange}
              error={formik.touched.email && Boolean(formik.errors.email)}
              helperText={formik.touched.email && formik.errors.email}
              sx={{}}
              required
            />
          </Grid>
        </Grid>

        {/* Beitrittstext */}
        <Typography variant="body1" fontWeight="bold" sx={{ mb: 2 }}>
          meinen Beitritt zur FFW Apfeltrang e.V.
        </Typography>

        {/* WhatsApp-Gruppe */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Typography variant="body1" fontWeight="bold" sx={{ mr: 2 }}>
            Interesse an der Vereins-WhatsApp-Gruppe:
          </Typography>
          <RadioGroup
            row
            name="whatsappGroup"
            value={formik.values.whatsappGroup}
            onChange={formik.handleChange}
          >
            <FormControlLabel value="ja" control={<Radio />} label="ja" />
            <FormControlLabel value="nein" control={<Radio />} label="nein" sx={{ '& .Mui-checked': { color: '#ff0000' } }} />
          </RadioGroup>
        </Box>

        {/* Ersteintritt in die Feuerwehr */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="body1" fontWeight="bold" sx={{ mb: 1 }}>
            Wann erfolgte der Ersteintritt in die Feuerwehr?
          </Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} sm={8}>
              <TextField
                fullWidth
                id="previousFireDepartment"
                name="previousFireDepartment"
                label="Name der vorherigen Feuerwehr"
                variant="standard"
                value={formik.values.previousFireDepartment || ''}
                onChange={formik.handleChange}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <DatePicker
                label="Eintrittsdatum"
                value={formik.values.entryDate}
                onChange={(value) => formik.setFieldValue('entryDate', value)}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    variant: 'standard'
                  },
                }}
              />
            </Grid>
          </Grid>
        </Box>

        {/* Interesse am aktiven Dienst */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <Typography variant="body1" fontWeight="bold" sx={{ mr: 2 }}>
            Interesse am aktiven Dienst:
          </Typography>
          <RadioGroup
            row
            name="activeMember"
            value={formik.values.activeMember}
            onChange={formik.handleChange}
          >
            <FormControlLabel value="ja" control={<Radio />} label="ja" />
            <FormControlLabel value="nein" control={<Radio />} label="nein" sx={{ '& .Mui-checked': { color: '#ff0000' } }} />
          </RadioGroup>
        </Box>

        {/* SEPA-Mandat */}
        <Typography variant="body1" sx={{ mb: 2 }}>
          Der Mitgliedsbeitrag wird durch ein <strong>SEPA-Lastschriftmandat</strong> jährlich eingezogen. (Unsere Gläubiger-ID: DE04ZZZ00001520663. Als Mandatsreferenz verwenden Name_Vorname_1 (Bei Namensgleichheit ist die Nummer fortlaufend). <strong>Ich gebe hierzu mein Einverständnis und ermächtige die FFW Apfeltrang e.V. den jeweils fälligen Beitrag einzuziehen.</strong>
        </Typography>

        {/* Bankdaten */}
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={12}>
            <TextField
              fullWidth
              id="accountHolder"
              name="accountHolder"
              label="Konto-Inhaber"
              variant="standard"
              value={formik.values.accountHolder || ''}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="bic"
              name="bic"
              label="BIC"
              variant="standard"
              value={formik.values.bic || ''}
              onChange={formik.handleChange}
              sx={{}}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="iban"
              name="iban"
              label="IBAN"
              variant="standard"
              value={formik.values.iban || ''}
              onChange={formik.handleChange}
              sx={{}}
            />
          </Grid>
        </Grid>

        {/* Beitragsregelung */}
        <Typography variant="body1" fontWeight="bold" sx={{ mb: 1 }}>
          Es gilt derzeit folgende Beitragsregelung:
        </Typography>
        <Typography variant="body1" sx={{ ml: 2, mb: 0.5 }}>- Jährlicher Mitgliedsbeitrag für Männer und Frauen: je 5,00 Euro</Typography>
        <Typography variant="body1" sx={{ ml: 2, mb: 0.5 }}>- Beitragspflicht besteht ab Vollendung des 18. Lebensjahres</Typography>
        <Typography variant="body1" sx={{ ml: 2, mb: 3 }}>- Mitglieder ab Vollendung des 60. Lebensjahres und mindestens 10 jähriger Mitgliedschaft können auf Antrag von der Beitragspflicht befreit werden</Typography>

        {/* Datenschutz */}
        <Typography variant="body1" sx={{ mb: 4 }}>
          Ich bin damit einverstanden, dass meine Daten aus Vereinszwecken elektronisch gespeichert werden.
        </Typography>

        {/* Unterschriftenfelder */}
        <Grid container spacing={3} sx={{ mb: 5 }}>
          <Grid item xs={12} sm={3}>
            <TextField
              fullWidth
              id="place"
              name="place"
              label="Ort"
              variant="standard"
              value={formik.values.place || ''}
              onChange={formik.handleChange}
              error={formik.touched.place && Boolean(formik.errors.place)}
              helperText={formik.touched.place && formik.errors.place}
              sx={{}}
              required
            />
            <DatePicker
              label="Datum"
              value={formik.values.signatureDate}
              onChange={(value) => formik.setFieldValue('signatureDate', value)}
              slotProps={{
                textField: {
                  fullWidth: true,
                  variant: 'standard',
                  margin: 'normal',
                  error: formik.touched.signatureDate && Boolean(formik.errors.signatureDate),
                  helperText: formik.touched.signatureDate && formik.errors.signatureDate,
                  required: true,
                  sx: {}
                },
              }}
            />
            <Typography variant="caption">Ort, Datum</Typography>
          </Grid>
          <Grid item xs={12} sm={4.5}>
            <SignatureField
              label="Unterschrift des Neumitgliedes"
              value={formik.values.signature}
              onChange={(value) => formik.setFieldValue('signature', value)}
              error={formik.touched.signature && Boolean(formik.errors.signature)}
              helperText={formik.touched.signature && formik.errors.signature}
            />
          </Grid>
          <Grid item xs={12} sm={4.5}>
            <SignatureField
              label="Unterschrift der Eltern (bei Minderjährigen)"
              value={formik.values.parentSignature}
              onChange={(value) => formik.setFieldValue('parentSignature', value)}
              error={formik.touched.parentSignature && Boolean(formik.errors.parentSignature)}
              helperText={formik.touched.parentSignature && formik.errors.parentSignature}
            />
          </Grid>
        </Grid>

        {/* Fußzeile */}
        <Grid container spacing={3}>
          <Grid item xs={12} sm={4}>
            <Typography variant="body2" fontWeight="bold">Stellvertretender Kommandant:</Typography>
            <Typography variant="body2">Simon Wintergerst</Typography>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Typography variant="body2" fontWeight="bold">2. Vorsitzender:</Typography>
            <Typography variant="body2">Matthias Heimrich</Typography>
            <Typography variant="body2" fontWeight="bold" sx={{ mt: 1 }}>Kassenwart:</Typography>
            <Typography variant="body2">Andreas Schneider</Typography>
            <Typography variant="body2" fontWeight="bold" sx={{ mt: 1 }}>Schriftführer:</Typography>
            <Typography variant="body2">Wolfgang Haberlitter</Typography>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Typography variant="body2" fontWeight="bold">Bankverbindung:</Typography>
            <Typography variant="body2">VR Bank Augsburg-Ostallgäu</Typography>
            <Typography variant="body2">IBAN: DE28720900000100303291</Typography>
          </Grid>
        </Grid>

        {/* Submit Button */}
        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
          <Button 
            type="submit" 
            variant="contained" 
            color="primary"
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Absenden'}
          </Button>
        </Box>
      </form>
    </LocalizationProvider>
  );
};

export default RegistrationForm; 