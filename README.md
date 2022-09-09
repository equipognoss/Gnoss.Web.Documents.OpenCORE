![](https://content.gnoss.ws/imagenes/proyectos/personalizacion/7e72bf14-28b9-4beb-82f8-e32a3b49d9d3/cms/logognossazulprincipal.png)

# Gnoss.Web.Documents.OpenCORE

![](https://github.com/equipognoss/Gnoss.Web.Documents.OpenCORE/workflows/BuildWeb/badge.svg)

Aplicación Web que se encarga de almacenar y servir los documentos que suben los usuarios a la plataforma, tales como archivos Word, PDF, hojas de cálculo, archivos comprimidos, etc. Esta aplicación NO debe ser accesible desde el exterior de la plataforma GNOSS, sólo debe estar disponible para que el resto de aplicaciones puedan hacer peticiones Web a ella.

Configuración estandar de esta aplicación en el archivo docker-compose.yml: 

```yml
documents:
    image: gnoss/documents
    env_file: .env
    ports:
     - ${puerto_documents}:80
    environment:
     AzureStorageConnectionString: ${AzureStorageConnectionString}
     LogLocation: ${LogLocation}
     ImplementationKey: ${ImplementationKey}
    volumes:
      - ./logs/documents:/app/logs
      - ./content/Documentacion:/app/Documentacion
```

En esta configuración, existe un volumen que apunta a la ruta /app/Documentacion del contenedor. Ese volumen almacenará todos los documentos (Word, Pdf, Ppt, Excel, etc.) subidos a la plataforma. Se recomienda realizar copias de seguridad de la unidad en la que se mapee ese directorio.

Se pueden consultar los posibles valores de configuración de cada parámetro aquí: https://github.com/equipognoss/Gnoss.SemanticAIPlatform.OpenCORE

## Código de conducta
Este proyecto a adoptado el código de conducta definido por "Contributor Covenant" para definir el comportamiento esperado en las contribuciones a este proyecto. Para más información ver https://www.contributor-covenant.org/

## Licencia
Este producto es parte de la plataforma [Gnoss Semantic AI Platform Open Core](https://github.com/equipognoss/Gnoss.SemanticAIPlatform.OpenCORE), es un producto open source y está licenciado bajo GPLv3.
