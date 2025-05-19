using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Controllers;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Server.Tests
{
    public class FormControllerTests
    {
        [Fact]
        public void SubmitForm_ValidModel_GeneratesPdfToLocalPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FormController>>();
            var envMock = new Mock<IWebHostEnvironment>();
            
            // Setup environment mock to return a proper content root path
            var contentRootPath = Path.Combine(Path.GetTempPath(), "FFWAnmeldungTests");
            Directory.CreateDirectory(contentRootPath);
            Directory.CreateDirectory(Path.Combine(contentRootPath, "Resources"));
            
            // Create a dummy logo file if it doesn't exist
            var logoPath = Path.Combine(contentRootPath, "Resources", "logo_shields.png");
            if (!File.Exists(logoPath))
            {
                // Create a simple 1x1 pixel file
                using (var fs = File.Create(logoPath))
                {
                    // Simple PNG header (not a valid PNG but enough for mock)
                    byte[] bytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            
            envMock.Setup(e => e.ContentRootPath).Returns(contentRootPath);
            
            var controller = new FormController(loggerMock.Object, envMock.Object);
            
            // Create test data
            var model = new MemberRegistrationModel
            {
                FirstName = "Test",
                LastName = "User",
                BirthDate = new DateTime(1990, 1, 1),
                Street = "Test Street 123",
                City = "Test City",
                PostalCode = "12345",
                Phone = "0123456789",
                Mobile = "0987654321",
                Email = "test@example.com",
                WhatsappGroup = true,
                PreviousFireDepartment = "Previous FD",
                EntryDate = new DateTime(2015, 1, 1),
                ActiveMember = true,
                AccountHolder = "Test User",
                BIC = "TESTBIC1XXX",
                IBAN = "DE123456789012345678",
                Place = "Test City",
                SignatureDate = DateTime.Now,
                Signature = "Heimrich"
            };
            
            // Define a local output path for the PDF
            var outputDir = Path.Combine(Path.GetTempPath(), "FFWAnmeldungTests", "Output");
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, $"Beitrittserklärung_Test_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            
            // Act
            // Direct call to CreatePdf using reflection to test the PDF generation directly
            var methodInfo = typeof(FormController).GetMethod("CreatePdf", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            methodInfo.Invoke(controller, new object[] { outputPath, model });
            
            // Assert
            Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
            Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");
            
            // Optional: Cleanup
            // File.Delete(outputPath);
            // Directory.Delete(outputDir, true);
            
            // Log the path so we can manually inspect the PDF
            Console.WriteLine($"PDF created at: {outputPath}");
        }
        
        [Fact]
        public void SubmitForm_ValidModel_ReturnsSuccessResult()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FormController>>();
            var envMock = new Mock<IWebHostEnvironment>();
            
            var contentRootPath = Path.Combine(Path.GetTempPath(), "FFWAnmeldungTests");
            Directory.CreateDirectory(contentRootPath);
            Directory.CreateDirectory(Path.Combine(contentRootPath, "Resources"));
            
            envMock.Setup(e => e.ContentRootPath).Returns(contentRootPath);
            
            var controller = new FormController(loggerMock.Object, envMock.Object);
            
            // Create test data
            var model = new MemberRegistrationModel
            {
                FirstName = "Test",
                LastName = "User",
                BirthDate = new DateTime(1990, 1, 1)
                // Basic data is enough for this test
            };
            
            // Act
            var result = controller.SubmitForm(model);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic resultValue = okResult.Value;
            
            Assert.True((bool)resultValue.Success);
            Assert.NotNull((string)resultValue.FilePath);
            Assert.NotNull((string)resultValue.FileName);
            
            // Verify the file exists
            Assert.True(File.Exists((string)resultValue.FilePath));
            
            // Cleanup
            File.Delete((string)resultValue.FilePath);
        }
    }
} 