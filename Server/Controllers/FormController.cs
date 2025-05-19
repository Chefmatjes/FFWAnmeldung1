using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// iText7 Namespaces
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using iText.Kernel.Geom;

// Explizites System.IO
using IOPath = System.IO.Path;
using System.IO;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly ILogger<FormController> _logger;
        private readonly IWebHostEnvironment _environment;

        public FormController(ILogger<FormController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("submit")]
        public IActionResult SubmitForm([FromBody] MemberRegistrationModel model)
        {
            try
            {
                _logger.LogInformation("Form submission received: {FirstName} {LastName}", model?.FirstName, model?.LastName);
                
                string fileName = $"Beitrittserklärung_{model?.LastName}_{model?.FirstName}_{DateTime.Now:yyyyMMdd}.pdf";
                string tempPath = IOPath.Combine(IOPath.GetTempPath(), fileName);
                
                // PDF-Generierung mit iText7
                CreatePdf(tempPath, model!);

                _logger.LogInformation("PDF generated and saved to: {FilePath}", tempPath);

                return Ok(new { Success = true, FilePath = tempPath, FileName = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form submission");
                return StatusCode(500, new { Error = "Failed to process form submission", Message = ex.Message });
            }
        }

        private void CreatePdf(string filePath, MemberRegistrationModel model)
        {
            // PDF-Writer initialisieren
            PdfWriter writer = new PdfWriter(filePath);
            
            // PdfDocument erstellen
            PdfDocument pdf = new PdfDocument(writer);
            
            // Document erstellen mit A4-Format
            Document document = new Document(pdf, PageSize.A4);
            
            try
            {
                // Seitenränder festlegen
                document.SetMargins(50, 50, 50, 50);
                
                // Schriftarten definieren
                PdfFont titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                
                // Logo und Header erstellen
                Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER);
                
                // Linke Spalte - Feuerwehr Name
                Cell leftHeaderCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0);
                
                Paragraph feuerwehrName = new Paragraph("Freiwillige Feuerwehr")
                    .SetFont(boldFont)
                    .SetFontSize(22)
                    .SetMarginBottom(0);
                
                Paragraph ortsname = new Paragraph("Apfeltrang e.V.")
                    .SetFont(boldFont)
                    .SetFontSize(22)
                    .SetMarginTop(0)
                    .SetMarginBottom(0);
                
                leftHeaderCell.Add(feuerwehrName);
                leftHeaderCell.Add(ortsname);
                headerTable.AddCell(leftHeaderCell);
                
                // Rechte Spalte - Logo und Kontaktdaten
                Cell rightHeaderCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .SetTextAlignment(TextAlignment.RIGHT);
                
                // Neues Logo einbinden (zwei Schilde)
                try
                {
                    // Get the executing assembly location for a more reliable path reference
                    string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string assemblyDirectory = IOPath.GetDirectoryName(assemblyLocation);
                    
                    _logger.LogInformation("Assembly location: {AssemblyLocation}", assemblyLocation);
                    _logger.LogInformation("Assembly directory: {AssemblyDirectory}", assemblyDirectory);
                    _logger.LogInformation("Content root path: {ContentRootPath}", _environment.ContentRootPath);
                    _logger.LogInformation("Web root path: {WebRootPath}", _environment.WebRootPath);
                    _logger.LogInformation("Current directory: {CurrentDirectory}", Environment.CurrentDirectory);
                    
                    // Try multiple potential logo paths
                    string[] potentialLogoPaths = new string[] {
                        // Original paths
                        IOPath.Combine(_environment.ContentRootPath, "Resources", "logo.png"),
                        // Assembly-based paths
                        IOPath.Combine(assemblyDirectory, "Resources", "logo.png"),
                        IOPath.Combine(assemblyDirectory, "Resources", "logo_shields.png"),
                        // Current directory based paths
                        IOPath.Combine(Environment.CurrentDirectory, "Resources", "logo.png"),
                        IOPath.Combine(Environment.CurrentDirectory, "Resources", "logo_shields.png"),
                        // Absolute fallback for development
                        @"C:\Projekte\FFWAnmeldung\FFWAnmeldung\Server\Resources\logo.png",
                    };
                    
                    string logoPath = null;
                    foreach (var path in potentialLogoPaths)
                    {
                        _logger.LogInformation("Checking logo at: {LogoPath}", path);
                        Console.WriteLine($"Checking logo at: {path}");
                        
                        if (System.IO.File.Exists(path))
                        {
                            logoPath = path;
                            _logger.LogInformation("Found logo at: {LogoPath}", logoPath);
                            Console.WriteLine($"Found logo at: {logoPath}");
                            break;
                        }
                    }
                    
                    if (logoPath != null)
                    {
                        try 
                        {
                            // Load and add the logo image
                            Image logo = new Image(ImageDataFactory.Create(logoPath))
                                .SetWidth(80)
                                .SetMarginBottom(10)
                                .SetMarginTop(5)
                                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                            rightHeaderCell.Add(logo);
                            _logger.LogInformation("Logo successfully added to PDF");
                        }
                        catch (Exception imgEx)
                        {
                            _logger.LogError(imgEx, "Error processing logo image: {LogoPath}", logoPath);
                            Console.WriteLine($"Error processing logo: {imgEx.Message}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No logo file found at any of the expected paths");
                        Console.WriteLine("No logo file found at any of the expected paths");
                        
                        // Create a fallback text element instead of the logo
                        Paragraph logoPlaceholder = new Paragraph("Logo")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetTextAlignment(TextAlignment.RIGHT);
                        rightHeaderCell.Add(logoPlaceholder);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading logo image");
                    Console.WriteLine($"Error loading logo: {ex.Message}");
                }
                
                // Kontaktdaten
                Table contactTable = new Table(1)
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER);
                
                // 1. Vorsitzender
                Paragraph vorsitzenderLabel = new Paragraph("1. Vorsitzender:")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetMarginBottom(0);
                
                Paragraph vorsitzenderName = new Paragraph("Michael Stich, Im Obstgarten 29\n87674 Ruderatshofen")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginTop(0)
                    .SetMarginBottom(0);
                
                // 1. Kommandant
                Paragraph kommandantLabel = new Paragraph("1. Kommandant:")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetMarginBottom(0);
                
                Paragraph kommandantName = new Paragraph("Pascal Reimann, Apfeltranger Dorfstr. 15\n87674 Ruderatshofen")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginTop(0)
                    .SetMarginBottom(0);
                
                // Email
                Paragraph emailLabel = new Paragraph("Mail:")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetMarginBottom(0);
                
                Paragraph emailAdresse = new Paragraph("Vorstand@ff-apfeltrang.de")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginTop(0);
                
                // Alle Kontaktdaten zur Tabelle hinzufügen
                Cell contactCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(vorsitzenderLabel)
                    .Add(vorsitzenderName)
                    .Add(kommandantLabel)
                    .Add(kommandantName)
                    .Add(emailLabel)
                    .Add(emailAdresse);
                
                contactTable.AddCell(contactCell);
                rightHeaderCell.Add(contactTable);
                
                headerTable.AddCell(rightHeaderCell);
                document.Add(headerTable);
                
                // Titel
                Paragraph title = new Paragraph("Beitrittserklärung")
                    .SetFont(titleFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginTop(20)
                    .SetMarginBottom(20);
                document.Add(title);
                
                // Erklärungstext
                Paragraph erklaerung = new Paragraph("hiermit erkläre ich,")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10);
                document.Add(erklaerung);
                
                // Formularfelder (erste Zeile, unterstrichen, Label darunter)
                Table formTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginBottom(5);
                
                Cell nameCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph(model?.FirstName + " " + model?.LastName ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("Vorname, Name")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                Cell addressCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph((model?.Street + ", " + model?.PostalCode + " " + model?.City) ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("Straße, Wohnort")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                Cell birthDateCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph(model?.BirthDate.ToString("dd.MM.yyyy") ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("Geburtsdatum")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                formTable.AddCell(nameCell);
                formTable.AddCell(addressCell);
                formTable.AddCell(birthDateCell);
                
                document.Add(formTable);
                
                // Zweite Zeile - Telefon, Mobil, Email (unterstrichen, Label darunter)
                Table formTable2 = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginBottom(10);
                
                Cell phoneCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph(model?.Phone ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("Telefon")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                Cell mobileCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph(model?.Mobile ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("Mobil")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                Cell emailCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .Add(new Paragraph(model?.Email ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("_______________________________")
                        .SetFontSize(11)
                        .SetMarginTop(-10)
                        .SetMarginBottom(0))
                    .Add(new Paragraph("E-Mail")
                        .SetFontSize(9)
                        .SetMarginTop(0));
                
                formTable2.AddCell(phoneCell);
                formTable2.AddCell(mobileCell);
                formTable2.AddCell(emailCell);
                document.Add(formTable2);
                
                // Beitritt Text
                Paragraph beitrittText = new Paragraph("meinen Beitritt zur FFW Apfeltrang e.V.")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginTop(10)
                    .SetMarginBottom(10);
                document.Add(beitrittText);
                
                // WhatsApp-Gruppe (Checkboxen klarer und nebeneinander)
                Table whatsappTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 10, 10, 20 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginBottom(0);

                // Create cells with proper drawn checkboxes instead of Unicode characters
                Cell whatsappJaCell = new Cell(1, 1).SetBorder(Border.NO_BORDER);
                Table jaCheckboxTable = new Table(1)
                    .SetBorder(Border.NO_BORDER)
                    .SetWidth(60);

                // Create checkbox cell for "ja"
                Cell jaBox = new Cell(1, 1)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                    .SetHeight(12)
                    .SetWidth(12)
                    .SetPadding(0)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Add X mark if selected
                if (model?.WhatsappGroup == true)
                {
                    jaBox.Add(new Paragraph("X")
                        .SetFontSize(9)
                        .SetFont(boldFont)
                        .SetMargin(0)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                // Create label cell with ja text
                Cell jaLabel = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .SetMarginLeft(5)
                    .Add(new Paragraph(" ja").SetFontSize(11));

                // Create one row table for the checkbox and label
                Table jaRowTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
                    .SetBorder(Border.NO_BORDER);
                jaRowTable.AddCell(jaBox);
                jaRowTable.AddCell(jaLabel);
                whatsappJaCell.Add(jaRowTable);

                // Create cell with checkbox for "nein"
                Cell whatsappNeinCell = new Cell(1, 1).SetBorder(Border.NO_BORDER);
                Table neinCheckboxTable = new Table(1)
                    .SetBorder(Border.NO_BORDER)
                    .SetWidth(60);

                // Create checkbox cell for "nein"
                Cell neinBox = new Cell(1, 1)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                    .SetHeight(12)
                    .SetWidth(12)
                    .SetPadding(0)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Add X mark if selected
                if (model?.WhatsappGroup == false)
                {
                    neinBox.Add(new Paragraph("X")
                        .SetFontSize(9)
                        .SetFont(boldFont)
                        .SetMargin(0)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                // Create label cell with nein text
                Cell neinLabel = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .SetMarginLeft(5)
                    .Add(new Paragraph(" nein").SetFontSize(11));

                // Create one row table for the checkbox and label
                Table neinRowTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
                    .SetBorder(Border.NO_BORDER);
                neinRowTable.AddCell(neinBox);
                neinRowTable.AddCell(neinLabel);
                whatsappNeinCell.Add(neinRowTable);

                whatsappTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("Interesse an der Vereins-WhatsApp-Gruppe:").SetFont(boldFont).SetFontSize(11)));
                whatsappTable.AddCell(whatsappJaCell);
                whatsappTable.AddCell(whatsappNeinCell);
                whatsappTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER));
                document.Add(whatsappTable);
                
                // Ersteintritt
                Table ersteintrittsTable = new Table(UnitValue.CreatePercentArray(new float[] { 40, 30, 30 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginTop(10);
                
                ersteintrittsTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("Wann erfolgte der Ersteintritt in die Feuerwehr?").SetFont(boldFont).SetFontSize(11)));
                ersteintrittsTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph(model?.PreviousFireDepartment ?? "________________________").SetFontSize(11).SetMarginBottom(0)).Add(new Paragraph("Name der vorherigen Feuerwehr").SetFontSize(9).SetMarginTop(0)));
                ersteintrittsTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph(model?.EntryDate?.ToString("dd.MM.yyyy") ?? "________________").SetFontSize(11).SetMarginBottom(0)).Add(new Paragraph("Eintrittsdatum").SetFontSize(9).SetMarginTop(0)));
                document.Add(ersteintrittsTable);
                
                // Interesse am aktiven Dienst (Checkboxen klarer und nebeneinander)
                Table aktiverDienstTable = new Table(UnitValue.CreatePercentArray(new float[] { 40, 10, 10, 40 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginTop(10);

                // Create cells with proper drawn checkboxes for active member
                Cell aktivJaCell = new Cell(1, 1).SetBorder(Border.NO_BORDER);
                Table aktivJaCheckboxTable = new Table(1)
                    .SetBorder(Border.NO_BORDER)
                    .SetWidth(60);

                // Create checkbox cell for "ja"
                Cell aktivJaBox = new Cell(1, 1)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                    .SetHeight(12)
                    .SetWidth(12)
                    .SetPadding(0)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Add X mark if selected
                if (model?.ActiveMember == true)
                {
                    aktivJaBox.Add(new Paragraph("X")
                        .SetFontSize(9)
                        .SetFont(boldFont)
                        .SetMargin(0)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                // Create label cell with ja text
                Cell aktivJaLabel = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .SetMarginLeft(5)
                    .Add(new Paragraph(" ja").SetFontSize(11));

                // Create one row table for the checkbox and label
                Table aktivJaRowTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
                    .SetBorder(Border.NO_BORDER);
                aktivJaRowTable.AddCell(aktivJaBox);
                aktivJaRowTable.AddCell(aktivJaLabel);
                aktivJaCell.Add(aktivJaRowTable);

                // Create cell with checkbox for "nein"
                Cell aktivNeinCell = new Cell(1, 1).SetBorder(Border.NO_BORDER);
                Table aktivNeinCheckboxTable = new Table(1)
                    .SetBorder(Border.NO_BORDER)
                    .SetWidth(60);

                // Create checkbox cell for "nein"
                Cell aktivNeinBox = new Cell(1, 1)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                    .SetHeight(12)
                    .SetWidth(12)
                    .SetPadding(0)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Add X mark if selected
                if (model?.ActiveMember == false)
                {
                    aktivNeinBox.Add(new Paragraph("X")
                        .SetFontSize(9)
                        .SetFont(boldFont)
                        .SetMargin(0)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                // Create label cell with nein text
                Cell aktivNeinLabel = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0)
                    .SetMarginLeft(5)
                    .Add(new Paragraph(" nein").SetFontSize(11));

                // Create one row table for the checkbox and label
                Table aktivNeinRowTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
                    .SetBorder(Border.NO_BORDER);
                aktivNeinRowTable.AddCell(aktivNeinBox);
                aktivNeinRowTable.AddCell(aktivNeinLabel);
                aktivNeinCell.Add(aktivNeinRowTable);

                aktiverDienstTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("Interesse am aktiven Dienst:").SetFont(boldFont).SetFontSize(11)));
                aktiverDienstTable.AddCell(aktivJaCell);
                aktiverDienstTable.AddCell(aktivNeinCell);
                aktiverDienstTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER));
                document.Add(aktiverDienstTable);
                
                // SEPA-Mandat und Beitragsregelung (wichtige Teile fett)
                Paragraph sepaText = new Paragraph()
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginTop(30)
                    .Add("Der Mitgliedsbeitrag wird durch ein ")
                    .Add(new Text("SEPA-Lastschriftmandat").SetFont(boldFont))
                    .Add(" jährlich eingezogen. (Unsere Gläubiger-ID: DE04ZZZ00001520663. Als Mandatsreferenz verwenden Name_Vorname_1 (Bei Namensgleichheit ist die Nummer fortlaufend). ")
                    .Add(new Text("Ich gebe hierzu mein Einverständnis und ermächtige die FFW Apfeltrang e.V. den jeweils fälligen Beitrag einzuziehen.").SetFont(boldFont));
                
                document.Add(sepaText);
                
                // SEPA-Details (unterstrichene Felder)
                Table sepaDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginTop(10);
                
                sepaDetailsTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("Konto-Inhaber:").SetFontSize(11)));
                sepaDetailsTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph((model?.AccountHolder ?? "_______________________________________________").ToString()).SetFontSize(11).SetUnderline()));
                document.Add(sepaDetailsTable);
                
                // BIC und IBAN (unterstrichene Felder)
                Paragraph bankingInfo = new Paragraph()
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .Add("BIC: ")
                    .Add(new Text((model?.BIC ?? "__ __ __ __ __ __ __ __ __ __ __")).SetUnderline())
                    .Add("    IBAN: DE ")
                    .Add(new Text((model?.IBAN ?? "__ __ / __ __ __ __ / __ __ __ __ / __ __ __ __ / __ __ __ __ / __ __")).SetUnderline())
                    .SetMarginTop(10);
                document.Add(bankingInfo);
                
                // Beitragsregelung
                Paragraph beitragsregelungHeader = new Paragraph("Es gilt derzeit folgende Beitragsregelung:")
                    .SetFont(boldFont)
                    .SetFontSize(11)
                    .SetMarginTop(15);
                document.Add(beitragsregelungHeader);
                
                // Beitragsregelung Punkte
                List beitragsregelungList = new List()
                    .SetSymbolIndent(12)
                    .SetListSymbol("- ")
                    .SetFont(normalFont)
                    .SetFontSize(11);
                
                beitragsregelungList.Add(new ListItem("Jährlicher Mitgliedsbeitrag für Männer und Frauen: je 5,00 Euro"));
                beitragsregelungList.Add(new ListItem("Beitragspflicht besteht ab Vollendung des 18. Lebensjahres"));
                beitragsregelungList.Add(new ListItem("Mitglieder ab Vollendung des 60. Lebensjahres und mindestens 10 jähriger Mitgliedschaft können auf Antrag von der Beitragspflicht befreit werden"));
                
                document.Add(beitragsregelungList);
                
                // Datenschutz
                Paragraph datenschutz = new Paragraph("Ich bin damit einverstanden, dass meine Daten aus Vereinszwecken elektronisch gespeichert werden.")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginTop(15);
                document.Add(datenschutz);
                
                // Unterschriftenbereich (drei Linien, Labels darunter)
                Table signaturesTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginTop(30);
                
                // Add date and location
                Cell dateLocationCell = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0);
                dateLocationCell.Add(new Paragraph(model?.Place + ", " + model?.SignatureDate?.ToString("dd.MM.yyyy") ?? "")
                    .SetFontSize(11)
                    .SetMarginBottom(0));
                dateLocationCell.Add(new Paragraph("_______________________________")
                    .SetFontSize(11)
                    .SetMarginTop(-10)
                    .SetMarginBottom(0));
                dateLocationCell.Add(new Paragraph("Ort, Datum")
                    .SetFontSize(9)
                    .SetMarginTop(0));
                signaturesTable.AddCell(dateLocationCell);

                // Add member signature
                Cell memberSignatureCell = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0);
                
                // Check if the signature is a base64 string
                if (model?.Signature != null && model.Signature.StartsWith("data:image"))
                {
                    try
                    {
                        // Extract the base64 data part
                        string base64Data = model.Signature.Split(',')[1];
                        byte[] signatureBytes = Convert.FromBase64String(base64Data);
                        
                        // Create an image from the signature bytes
                        Image signatureImage = new Image(ImageDataFactory.Create(signatureBytes))
                            .SetWidth(150)  // Set width to match the cell width
                            .SetMarginBottom(2);
                        
                        memberSignatureCell.Add(signatureImage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing signature image");
                        // Fallback to text if there's an error
                        memberSignatureCell.Add(new Paragraph("Unterschrift digital erfasst")
                            .SetFontSize(11)
                            .SetMarginBottom(0));
                    }
                }
                else
                {
                    // If it's not a base64 string, use the old text approach
                    memberSignatureCell.Add(new Paragraph(model?.Signature ?? "")
                        .SetFontSize(11)
                        .SetMarginBottom(0));
                }
                
                memberSignatureCell.Add(new Paragraph("_______________________________")
                    .SetFontSize(11)
                    .SetMarginTop(2)
                    .SetMarginBottom(0));
                memberSignatureCell.Add(new Paragraph("Unterschrift des Neumitgliedes")
                    .SetFontSize(9)
                    .SetMarginTop(0));
                signaturesTable.AddCell(memberSignatureCell);

                // Add parent signature if available
                Cell parentSignatureCell = new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(0);
                
                if (model?.ParentSignature != null)
                {
                    // Check if the parent signature is a base64 string
                    if (model.ParentSignature.StartsWith("data:image"))
                    {
                        try
                        {
                            // Extract the base64 data part
                            string base64Data = model.ParentSignature.Split(',')[1];
                            byte[] signatureBytes = Convert.FromBase64String(base64Data);
                            
                            // Create an image from the signature bytes
                            Image signatureImage = new Image(ImageDataFactory.Create(signatureBytes))
                                .SetWidth(150)  // Set width to match the cell width
                                .SetMarginBottom(2);
                            
                            parentSignatureCell.Add(signatureImage);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing parent signature image");
                            // Fallback to text if there's an error
                            parentSignatureCell.Add(new Paragraph("Unterschrift digital erfasst")
                                .SetFontSize(11)
                                .SetMarginBottom(0));
                        }
                    }
                    else
                    {
                        // If it's not a base64 string, use the old text approach
                        parentSignatureCell.Add(new Paragraph(model.ParentSignature)
                            .SetFontSize(11)
                            .SetMarginBottom(0));
                    }
                }
                else
                {
                    // Empty signature if not provided
                    parentSignatureCell.Add(new Paragraph(" ")
                        .SetFontSize(11)
                        .SetMarginBottom(0));
                }
                
                parentSignatureCell.Add(new Paragraph("_______________________________")
                    .SetFontSize(11)
                    .SetMarginTop(2)
                    .SetMarginBottom(0));
                parentSignatureCell.Add(new Paragraph("Unterschrift der Eltern bei Minderjährigen")
                    .SetFontSize(9)
                    .SetMarginTop(0));
                signaturesTable.AddCell(parentSignatureCell);

                document.Add(signaturesTable);
                
                // Fußzeile mit Funktionären (drei Spalten, wie Original)
                Table footerTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 }))
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginTop(30);
                
                // Stellvertretender Kommandant
                Cell stellvKommandantCell = new Cell().SetBorder(Border.NO_BORDER);
                
                stellvKommandantCell.Add(new Paragraph("Stellvertretender Kommandant:").SetFont(boldFont).SetFontSize(10));
                stellvKommandantCell.Add(new Paragraph("Simon Wintergerst").SetFont(normalFont).SetFontSize(10));
                
                // 2. Vorsitzender und Schriftführer
                Cell vorsitzenderCell = new Cell().SetBorder(Border.NO_BORDER);
                
                vorsitzenderCell.Add(new Paragraph("2. Vorsitzender:").SetFont(boldFont).SetFontSize(10));
                vorsitzenderCell.Add(new Paragraph("Matthias Heimrich").SetFont(normalFont).SetFontSize(10));
                
                vorsitzenderCell.Add(new Paragraph("Kassenwart:").SetFont(boldFont).SetFontSize(10).SetMarginTop(2));
                vorsitzenderCell.Add(new Paragraph("Andreas Schneider").SetFont(normalFont).SetFontSize(10));
                
                vorsitzenderCell.Add(new Paragraph("Schriftführer:").SetFont(boldFont).SetFontSize(10).SetMarginTop(2));
                vorsitzenderCell.Add(new Paragraph("Wolfgang Haberlitter").SetFont(normalFont).SetFontSize(10));
                
                // Bankverbindung
                Cell bankverbindungCell = new Cell().SetBorder(Border.NO_BORDER);
                
                bankverbindungCell.Add(new Paragraph("Bankverbindung:").SetFont(boldFont).SetFontSize(10));
                bankverbindungCell.Add(new Paragraph("VR Bank Augsburg-Ostallgäu").SetFont(normalFont).SetFontSize(10));
                bankverbindungCell.Add(new Paragraph("IBAN: DE28720900000100303291").SetFont(normalFont).SetFontSize(10));
                
                footerTable.AddCell(stellvKommandantCell);
                footerTable.AddCell(vorsitzenderCell);
                footerTable.AddCell(bankverbindungCell);
                
                document.Add(footerTable);
            }
            finally
            {
                // Ressourcen freigeben
                document.Close();
                pdf.Close();
                writer.Close();
            }
        }

        private void AddTableRow(Table table, string label, string value, PdfFont font)
        {
            Cell labelCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetFont(font)
                .SetFontSize(11)
                .Add(new Paragraph(label));
            
            Cell valueCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetFont(font)
                .SetFontSize(11)
                .Add(new Paragraph(value));
            
            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }
    }

    public class MemberRegistrationModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? MembershipType { get; set; }
        public bool? WhatsappGroup { get; set; }
        public string? PreviousFireDepartment { get; set; }
        public DateTime? EntryDate { get; set; }
        public bool? ActiveMember { get; set; }
        public string? AccountHolder { get; set; }
        public string? BIC { get; set; }
        public string? IBAN { get; set; }
        public string? Place { get; set; }
        public DateTime? SignatureDate { get; set; }
        public string? Signature { get; set; }
        public string? ParentSignature { get; set; }
    }
} 