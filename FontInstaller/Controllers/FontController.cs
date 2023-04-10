using FontInstaller.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace FontInstaller.Controllers
{
    public class FontController : Controller
    {
        private static string _installationDirectory;
        private string[] _acceptedFontFiles = {".ttf", ".gxf", ".fot", ".sfd", ".pfb", ".fnt", ".vfb", ".pfa", ".etx", ".woff", ".jfproj", ".vlw"};

        // GET: Font
        [HttpGet]
        public ActionResult Install()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Install(HttpPostedFileBase font)
        {
            try
            {
                _installationDirectory = Server.MapPath("~/assets/InstalledFonts");

                string path = Path.Combine(_installationDirectory,
                                            Path.GetFileName(font.FileName));

                if (!IsFontFile(new FileInfo(path)))
                {
                    return View("Error");
                }

                if (!Directory.Exists(Server.MapPath(_installationDirectory)))
                {
                    Directory.CreateDirectory(_installationDirectory);
                }

                font.SaveAs(path);

                RegisterFont(path, font.FileName);
            }
            catch (Exception)
            {
                // TODO: loging
                return View("Error");
            }
            
            return View();
        }


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);
        private bool RegisterFont(string sourcePath, string contentFontName)
        {
            try
            {
                // Creates the full path where your font will be installed
                var fontDestination = Path.Combine(System.Environment.GetFolderPath
                                                  (System.Environment.SpecialFolder.Fonts), contentFontName);

                if (!System.IO.File.Exists(fontDestination))
                {
                    // Copies font to destination
                    System.IO.File.Copy(sourcePath, fontDestination);

                    // Retrieves font name
                    // Makes sure you reference System.Drawing
                    PrivateFontCollection fontCol = new PrivateFontCollection();
                    fontCol.AddFontFile(fontDestination);
                    var actualFontName = fontCol.Families[0].Name;

                    //Add font
                    AddFontResource(fontDestination);
                    //Add registry entry  
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts",
                    actualFontName, contentFontName, RegistryValueKind.String);
                    
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }
 
        private bool IsFontFile(FileInfo file)
        {
            return _acceptedFontFiles.Contains(file.Extension);
        }
    }
}