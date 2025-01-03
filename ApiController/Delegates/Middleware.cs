namespace OpenRestClient;

public delegate void Middleware(HttpClient httpClient, HttpResponseMessage httpResponse, HttpRequest httpRequest);
