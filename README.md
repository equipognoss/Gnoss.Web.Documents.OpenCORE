![](https://content.gnoss.ws/imagenes/proyectos/personalizacion/7e72bf14-28b9-4beb-82f8-e32a3b49d9d3/cms/logognossazulprincipal.png)

# Gnoss.Web.Documents.OpenCORE

Aplicación Web que se encarga de almacenar y servir los documentos que suben los usuarios a la plataforma, tales como archivos Word, PDF, hojas de cálculo, archivos comprimidos, etc. Esta aplicación NO debe ser accesible desde el exterior de la plataforma GNOSS, sólo debe estar disponible para que el resto de aplicaciones puedan hacer peticiones Web a ella.

Configuración estandar de esta aplicación en el archivo docker-compose.yml: 

```yml
documents:
    image: docker.gnoss.com/documents
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

Se pueden consultar los posibles valores de configuración de cada parámetro aquí: https://github.com/equipognoss/Gnoss.Platform.Deploy
