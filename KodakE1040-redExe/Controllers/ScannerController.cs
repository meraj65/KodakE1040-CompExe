using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WIA;
using CommonDialog = WIA.CommonDialog;

namespace KodakE1040_redExe.Controllers
{
    [RoutePrefix("scannerapi")]
    public class ScannerController : ApiController
    {
        private readonly string query;
        private readonly string FormatType;
        private readonly string FileName;
        private readonly string OutPutFormat;
        private readonly int Pixel;
        private readonly int Dpi;
        private readonly int ColorMode;

        public ScannerController()
        {
            query = "SELECT * FROM Win32_PnPEntity WHERE Description LIKE '%Fax%' OR Description LIKE '%Scanner%'";
            FormatType = "image/tiff";
            OutPutFormat = "image/tiff";
            Pixel = 8;
            Dpi = 200;
            ColorMode = 1;
            FileName = "ScannedFile.tiff";
        }

        /// <summary>
        /// Scans an image and returns the base64-encoded content.
        /// </summary>
        [HttpGet]
        [Route("base64_scan")]
        public HttpResponseMessage Base64Scan()
        {
            var response = new
            {
                stage = "",
                status = "",
                message = "",
                scannedContentBase64 = ""
            };

            try
            {
                // ✅ Step 1: Get the scanner device
                response = new { stage = "GET_SCANNER", status = "PROCESSING", message = "Searching for scanner...", scannedContentBase64 = "" };
                DeviceManager deviceManager = new DeviceManager();
                DeviceInfo scannerDevice = GetScannerDevice(deviceManager);

                if (scannerDevice == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { stage = "GET_SCANNER", status = "ERROR", message = "No scanner found!", scannedContentBase64 = "" });
                }

                // ✅ Step 2: Connect to the scanner
                response = new { stage = "CONNECT_TO_SCANNER", status = "PROCESSING", message = "Connecting to scanner...", scannedContentBase64 = "" };
                Device scanner = scannerDevice.Connect();

                // ✅ Step 3: Start scanning
                response = new { stage = "GET_SCANNED_IMAGE", status = "PROCESSING", message = "Scanning in progress...", scannedContentBase64 = "" };
                Item scanItem = scanner.Items[1];

                SetScannerProperties(scanItem);

                CommonDialog dialog = new CommonDialog();
                ImageFile imageFile = (ImageFile)dialog.ShowTransfer(scanItem, FormatType, false);
                byte[] imageBytes = (byte[])imageFile.FileData.get_BinaryData();
                string base64String = Convert.ToBase64String(imageBytes);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    stage = "COMPLETED",
                    status = "SUCCESS",
                    message = "Scanning completed successfully.",
                    scannedContentBase64 = base64String
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    stage = response.stage,
                    status = "ERROR",
                    message = ex.Message,
                    scannedContentBase64 = ""
                });
            }
        }

        /// <summary>
        /// Scans an image and returns the binary content.
        /// </summary>
        [HttpGet]
        [Route("binary_scan")]
        public HttpResponseMessage BinaryScan()
        {
            var response = new
            {
                stage = "",
                status = "",
                message = "",
                scannedContentBinary = new byte[0]
            };

            try
            {
                // ✅ Step 1: Get the scanner device
                response = new { stage = "GET_SCANNER", status = "PROCESSING", message = "Searching for scanner...", scannedContentBinary = new byte[0] };
                DeviceManager deviceManager = new DeviceManager();
                DeviceInfo scannerDevice = GetScannerDevice(deviceManager);

                if (scannerDevice == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { stage = "GET_SCANNER", status = "ERROR", message = "No scanner found!", scannedContentBinary = new byte[0] });
                }

                // ✅ Step 2: Connect to the scanner
                response = new { stage = "CONNECT_TO_SCANNER", status = "PROCESSING", message = "Connecting to scanner...", scannedContentBinary = new byte[0] };
                Device scanner = scannerDevice.Connect();

                // ✅ Step 3: Start scanning
                response = new { stage = "GET_SCANNED_IMAGE", status = "PROCESSING", message = "Scanning in progress...", scannedContentBinary = new byte[0] };
                Item scanItem = scanner.Items[1];

                SetScannerProperties(scanItem);

                CommonDialog dialog = new CommonDialog();
                ImageFile imageFile = (ImageFile)dialog.ShowTransfer(scanItem, FormatType, false);
                byte[] imageBytes = (byte[])imageFile.FileData.get_BinaryData();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    stage = "COMPLETED",
                    status = "SUCCESS",
                    message = "Scanning completed successfully.",
                    scannedContentBinary = imageBytes
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    stage = response.stage,
                    status = "ERROR",
                    message = ex.Message,
                    scannedContentBinary = new byte[0]
                });
            }
        }

        /// <summary>
        /// Retrieves the first available scanner device.
        /// </summary>
        private DeviceInfo GetScannerDevice(DeviceManager deviceManager)
        {
            for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++) // WIA uses 1-based index
            {
                if (deviceManager.DeviceInfos[i].Type == WiaDeviceType.ScannerDeviceType)
                {
                    return deviceManager.DeviceInfos[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets scanner properties for color mode, resolution, and pixel depth.
        /// </summary>
        private void SetScannerProperties(Item scanItem)
        {
            SetScannerProperty(scanItem.Properties, 6146, ColorMode); // Color mode: 1 = B/W, 2 = Grayscale, 3 = Color
            SetScannerProperty(scanItem.Properties, 6147, Dpi); // DPI (Resolution)
            SetScannerProperty(scanItem.Properties, 6148, Pixel); // Bits per pixel
        }

        /// <summary>
        /// Sets a property on the scanner.
        /// </summary>
        private void SetScannerProperty(IProperties properties, int propertyID, int value)
        {
            try
            {
                foreach (Property property in properties)
                {
                    if (property.PropertyID == propertyID)
                    {
                        property.set_Value(value);
                        return;
                    }
                }
                Console.WriteLine($"Warning: Property {propertyID} not found on this scanner.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set property {propertyID}: {ex.Message}");
            }
        }
    }
}
