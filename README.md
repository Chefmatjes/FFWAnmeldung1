# Feuerwehr Anmeldung

Eine Webanwendung zur Online-Anmeldung für die Freiwillige Feuerwehr mit PDF-Generierung und automatisierter Übermittlung per E-Mail.

## Funktionen

- Webformular zur Datenerfassung für die Feuerwehr-Anmeldung
- PDF-Generierung aus den eingegebenen Daten
- Übermittlung per E-Mail an eine konfigurierte Adresse
- Responsive Design für alle Geräte
- Docker-Support für einfaches Deployment

## Technologie-Stack

- **Frontend**: React mit Material-UI für ein modernes und responsives UI
- **Backend**: ASP.NET Core für robuste API-Dienste
- **PDF-Generierung**: iText7
- **E-Mail-Versand**: .NET SMTP-Client
- **Deployment**: Docker und docker-compose

## Voraussetzungen

- Docker und docker-compose
- Für die Entwicklung:
  - .NET SDK 8.0 oder höher
  - Node.js 18 oder höher
  - npm oder yarn

## Konfiguration

Bevor Sie die Anwendung starten, müssen Sie die folgenden Konfigurationen vornehmen:

### E-Mail-Konfiguration

**WICHTIG: Speichern Sie niemals sensible Daten wie Passwörter direkt in der Versionskontrolle!**

Für die E-Mail-Konfiguration gibt es drei sichere Methoden:

#### 1. Für Entwicklung: .NET User Secrets

```bash
cd Server
dotnet user-secrets init
dotnet user-secrets set "Email:SmtpServer" "Ihr-SMTP-Server"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:SenderEmail" "absender@ihre-domain.de"
dotnet user-secrets set "Email:SenderName" "Feuerwehr Anmeldung"
dotnet user-secrets set "Email:SenderPassword" "Ihr-Passwort"
dotnet user-secrets set "Email:DefaultRecipientEmail" "empfang@ihre-domain.de"
```

#### 2. Für Produktion: Umgebungsvariablen

Setzen Sie folgende Umgebungsvariablen:

```
Email__SmtpServer=Ihr-SMTP-Server
Email__SmtpPort=587
Email__SenderEmail=absender@ihre-domain.de
Email__SenderName=Feuerwehr Anmeldung
Email__SenderPassword=Ihr-Passwort
Email__DefaultRecipientEmail=empfang@ihre-domain.de
```

#### 3. Für Docker-Deployment: Environment-Datei

Erstellen Sie eine `.env`-Datei (die nicht in Git eingecheckt wird):

```
EMAIL_SMTP_SERVER=Ihr-SMTP-Server
EMAIL_SMTP_PORT=587
EMAIL_SENDER=absender@ihre-domain.de
EMAIL_SENDER_NAME=Feuerwehr Anmeldung
EMAIL_PASSWORD=Ihr-Passwort
EMAIL_RECIPIENT=empfang@ihre-domain.de
```

Und verweisen Sie in Ihrer `docker-compose.yml` darauf:

```yaml
services:
  server:
    # ...
    env_file: .env
    environment:
      - Email__SmtpServer=${EMAIL_SMTP_SERVER}
      - Email__SmtpPort=${EMAIL_SMTP_PORT}
      - Email__SenderEmail=${EMAIL_SENDER}
      - Email__SenderName=${EMAIL_SENDER_NAME}
      - Email__SenderPassword=${EMAIL_PASSWORD}
      - Email__DefaultRecipientEmail=${EMAIL_RECIPIENT}
```

## Installation und Start

### Mit Docker (empfohlen)

1. Klonen Sie das Repository
   ```bash
   git clone <repository-url>
   cd FFWAnmeldung
   ```

2. Erstellen Sie eine `.env`-Datei mit den notwendigen Umgebungsvariablen (siehe oben)

3. Starten Sie die Container mit docker-compose
   ```bash
   docker-compose up -d
   ```

4. Die Anwendung ist nun verfügbar unter:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000

### Für Entwickler: Lokale Ausführung

#### Backend starten
```bash
cd Server
dotnet restore
# Stellen Sie sicher, dass Sie die User Secrets konfiguriert haben
dotnet run
```

#### Frontend starten
```bash
cd ClientApp
npm install
npm run dev
```

## Deployment

Diese Anwendung kann auf verschiedenen Plattformen bereitgestellt werden:

### Azure App Service

1. Erstellen Sie einen Azure App Service Plan
2. Erstellen Sie zwei App Services (eine für Frontend, eine für Backend)
3. Konfigurieren Sie die Verbindung zwischen den beiden Diensten
4. Konfigurieren Sie die Anwendungseinstellungen (App Settings) für die E-Mail-Konfiguration
5. Deployen Sie mit Docker oder direkt aus der Quellcodeversion

### Vercel (nur Frontend)

Das Frontend kann einfach auf Vercel bereitgestellt werden:

1. Verbinden Sie Ihr GitHub-Repository mit Vercel
2. Konfigurieren Sie die Build-Einstellungen
3. Stellen Sie das Backend separat bereit (z.B. auf Azure)

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert. 