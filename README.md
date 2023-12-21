 - Esta libreria se encarga de consumir una ApiRest de una manera facil y rapida
 - https://github.com/Clihsman/OpenRestClient.git

 - El RestController nos permite crear un controlador
 - para consumir la API 
 - RestController(<como argumento ingresamos la ruta que vamos a consumir>)
 
 - El RestMethod nos permite definir el tipo que queremos consumir
 - RestMethod(<tipo de metodo a consumir>)

 - El InJoin es una herramienta que nos permite unir un parametro 
 - ejemplo: 
   - long All(InJoin(",") params int[] ids) => Call<long>(nameof(Destroy), id);
   -  All(1,2,3,4); = https://host-prueba/api/metodo-prueba/1,2,3,4

 - El RestHost nos permine definir el Host que vamos a consumir
 - implementacion: [assembly: RestHost("https://host-prueba")]

 - El MethodType nos permite definir que tipo de metodo queremos usar

 - La clase RestApp nos permite contiene toda la estructura para consumir la API
    - RestApp(<tipo-clase-padre>)
    - RestApp(<host>, <tipo-clase-padre>)
    - Call<tipo-de-retorno>(<nombre-del-metodo>, <argumentos>); retorna el valor de T
    - CallString(<nombre-del-metodo>, <argumentos>); retorna el valor en String
    - CallArray<tipo-de-retorno>(<nombre-del-metodo>, <argumentos>); retorna el valor de T[] pero como un arreglo
    - AddHeader(<nombre>, <valor>); agrega un encabezado a la clase
