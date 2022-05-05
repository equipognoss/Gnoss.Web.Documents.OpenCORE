using Es.Riam.Gnoss.FileManager;
using Es.Riam.Gnoss.Util.General;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Gnoss.Web.Documents.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnoss.Web.Documents.Controllers
{
    [Route("GestorDocumental")]
    public class GestorDocumentalController : Controller
    {
        private static string mRutaFicheros;

        private static string mAzureStorageConnectionString;

        private GestionArchivos mGestorArchivos = null;

        private IHostingEnvironment _env;

        private LoggingService _loggingService;

        private ConfigService _configService;

        private static bool mEncriptacionActiva = true;

        public GestorDocumentalController(LoggingService loggingService, ConfigService configService, IHostingEnvironment env)
        {
            _configService = configService;
            _loggingService = loggingService;
            _env = env;
        }

        private GestionArchivos GestorArchivos
        {
            get
            {
                if (mGestorArchivos == null)
                {
                    string rutaConfigs = Path.Combine(_env.ContentRootPath, "config");
                    //string rutaConfigs = HttpContext.Current.Server.MapPath("/config");
                    mAzureStorageConnectionString = _configService.GetAzureStorageConnectionString();
                    if (string.IsNullOrEmpty(mAzureStorageConnectionString))
                    {
                        mAzureStorageConnectionString = "";
                        mRutaFicheros = Path.Combine(_env.ContentRootPath, "Documentacion");
                        //mRutaFicheros = HttpContext.Current.Server.MapPath("Documentacion");
                    }
                    if (System.IO.File.Exists(Path.Combine(rutaConfigs, "encript-disabled.config")))
                    {
                        //Comprobamos si esta configurada la desabilitar de la encriptacion
                        if (System.IO.File.ReadAllText(Path.Combine(rutaConfigs, "encript-disabled.config")).ToLower().Replace("\r", "").Replace("\n", "").Equals("true"))
                        {
                            mEncriptacionActiva = false;
                        }
                    }

                    mGestorArchivos = new GestionArchivos(_loggingService, mRutaFicheros, mAzureStorageConnectionString);
                }
                return mGestorArchivos;
            }
        }

        [HttpGet, Route("GetFile")]
        public IActionResult GetFile(string Name, string Extension, string Path)
        {
            try
            {
                return Ok(GestorArchivos.DescargarFichero(Path, Name + Extension, mEncriptacionActiva).Result);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al obtener el fichero {Name} con extension {Extension} en la ruta {Path}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
                return null;
            }
        }

        [HttpPost, Route("SetFile")]
        public IActionResult SetFile(IFormFile FileBytes, string Name, string Extension, string Path)
        {
            try
            {
                byte[] fileBytes = null;
                StringValues value;
                if (HttpContext.Request.Query.TryGetValue("FileBytes", out value))
                {
                    fileBytes = Convert.FromBase64String(value.ToString());
                }
                else
                {
                    if (FileBytes.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            FileBytes.CopyTo(ms);
                            fileBytes = ms.ToArray();
                            // act on the Base64 data
                        }
                    }
                }

                GestorArchivos.CrearDirectorioFisico(Path);
                //Crear el fichero en la ruta especificada
                GestorArchivos.CrearFicheroFisico(Path, Name + Extension, fileBytes, mEncriptacionActiva);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al crear el fichero {Name} con extension {Extension} en la ruta {Path}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
                return BadRequest("Error");
            }
            return Ok("");
        }

        [HttpPost, Route("DeleteFile")]
        public IActionResult DeleteFile(string Name, string Extension, string Path)
        {
            try
            {
                GestorArchivos.EliminarFicheroFisico(Path, Name + Extension);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al borrar el fichero {Name} con extension {Extension} en la ruta {Path}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
                return Ok(false);
            }
            return Ok(true);
        }

        [HttpPost, Route("DeleteFilesDirectory")]
        public IActionResult DeleteFilesDirectory(string Path)
        {
            try
            {
                if (!string.IsNullOrEmpty(Path) && mGestorArchivos.ComprobarExisteDirectorio(Path).Result)
                {
                    mGestorArchivos.EliminarDirectorioEnCascada(Path);
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al borrar los ficheros de la ruta {Path}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
            }
            return Ok(false);
        }

        [HttpPost, Route("DeleteFilesOntology")]
        public IActionResult DeleteFilesOntology(Guid Ontology)
        {
            try
            {
                string directorio = Path.Combine("DocumentosSemanticos", Ontology.ToString().Substring(0, 3), Ontology.ToString());

                mGestorArchivos.EliminarDirectorioEnCascada(directorio);

                return Ok(true);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al borrar los ficheros de la ontología {Ontology}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
            }
            return Ok(false);
        }

        [HttpPost, Route("CopyFile")]
        public IActionResult Copy(string PathOrigin, string PathDestination, string Name, string NameDestination, string Extension)
        {
            if (PathOrigin != null && PathDestination != null)
            {
                try
                {
                    if (!PathOrigin.Equals(PathDestination))
                    {
                        GestorArchivos.CrearDirectorioFisico(PathDestination);
                    }

                    GestorArchivos.CopiarArchivo(PathOrigin, PathDestination, Name + Extension, true, NameDestination);

                    return Ok(true);
                }
                catch (Exception ex)
                {
                    string mensajeExtra = $"Error al copiar el fichero {Name} con extension {Extension} desde la ruta {PathOrigin} a la ruta {PathDestination}";
                    _loggingService.GuardarLogError(ex, mensajeExtra);
                    return Ok(false);
                }
            }
            else
            {
                return Ok(false);
            }
        }

        [HttpPost, Route("MoveFile")]
        public IActionResult Move(string PathOrigin, string PathDestination, string Name, string Extension)
        {
            if (PathOrigin != null && PathDestination != null)
            {
                try
                {
                    GestorArchivos.CrearDirectorioFisico(PathDestination);

                    GestorArchivos.CopiarArchivo(PathOrigin, PathDestination, Name + Extension, false);

                    return Ok(true);
                }
                catch (Exception ex)
                {
                    string mensajeExtra = $"Error al mover el fichero {Name} con extension {Extension} desde la ruta {PathOrigin} a la ruta {PathDestination}";
                    _loggingService.GuardarLogError(ex, mensajeExtra);
                    return Ok(false);
                }
            }
            else
            {
                return Ok(false);
            }
        }

        [HttpPost, Route("CopyDocsDirectory")]
        public IActionResult CopyDocsDirectory(string PathOrigin, string PathDestination)
        {
            try
            {
                string rutaFicheroOrigen = PathOrigin;

                if (mGestorArchivos.ComprobarExisteDirectorio(rutaFicheroOrigen).Result)
                {
                    string[] ficheros = mGestorArchivos.ObtenerFicherosDeDirectorio(rutaFicheroOrigen).Result;

                    if (ficheros.Length > 0)
                    {
                        string rutaFicheroDestino = PathDestination;

                        bool exists = mGestorArchivos.ComprobarExisteDirectorio(rutaFicheroDestino).Result;
                        if (!exists)
                        {
                            mGestorArchivos.CrearDirectorioFisico(rutaFicheroDestino);
                        }

                        foreach (string fichero in ficheros)
                        {
                            mGestorArchivos.CopiarArchivo(rutaFicheroOrigen, rutaFicheroDestino, fichero, true);
                        }
                    }
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al copiar los ficheros desde la ruta {PathOrigin} a la ruta {PathDestination}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
                return Ok(false);
            }
        }


        [HttpGet, Route("GetFilesName")]
        public IActionResult GetFilesName(string Path)
        {
            List<string> listaNombreFicheros = null;

            if (Path[Path.Length - 1] != '/' || Path[Path.Length - 1] != '\\')
            {
                Path = Path.Substring(0, Path.Length - 1);
            }

            if (GestorArchivos.ComprobarExisteDirectorio(Path).Result)
            {
                string[] aux = GestorArchivos.ObtenerFicherosDeDirectorio(Path).Result;
                listaNombreFicheros = aux.ToList();
            }
            else
            {
                listaNombreFicheros = new List<string>();
                //AgregarErrorLog(new Exception("No se encontro el directorio"), rutaDirectorio);
            }

            return Ok(listaNombreFicheros);
        }

        [HttpGet, Route("GetDirectoriesName")]
        public IActionResult GetDirectoriesName(string Path)
        {
            List<string> listaNombreDirectorios = null;

            if (Path.EndsWith("/") || Path.EndsWith("\\"))
            {
                Path = Path.Substring(0, Path.Length - 1);
            }

            if (GestorArchivos.ComprobarExisteDirectorio(Path).Result)
            {
                string[] aux = GestorArchivos.ObtenerSubdirectoriosDeDirectorio(Path).Result;
                listaNombreDirectorios = aux.ToList();
            }
            else
            {
                listaNombreDirectorios = new List<string>();
            }

            return Ok(listaNombreDirectorios);
        }

        [HttpGet, Route("GetSizeFile")]
        public IActionResult GetSizeFile(string Name, string Extension, string Path)
        {
            try
            {
                long espacio = mGestorArchivos.ObtenerTamanioArchivo(Path, Name + Extension).Result;
                double espacioMB = 0;

                if (espacio > 0)
                {
                    espacioMB = ((double)espacio) / 1024 / 1024;
                }
                return Ok(espacioMB);
            }
            catch (Exception ex)
            {
                string mensajeExtra = $"Error al obtener el tamaño del fichero {Name} con extension {Extension} en la ruta {Path}";
                _loggingService.GuardarLogError(ex, mensajeExtra);
                return Ok(0);
            }
        }
    }
}
