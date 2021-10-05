# Cryptocurrency hedger API sample

This project contains a cryptocurrency hedger API sample which, in short, distributes a single cryptocurrency order between different exchange platform order books while also accounting for a particular exchange platform balance.

## Executing tests

Review tests, then execute ```dotnet test``` in the project's root folder.

## Testing API locally

Open Hedger.Api folder, execute ```dotnet run``` and navigate to `http://localhost:5000/swagger/index.html`.

The sample is caching order books from three fictitious exchanges: PipiExchange, MelkijadExchange and BarnExchange. Each offers 1.5 BTC as bid and ask positions, respectively, split in 0.5 BTC and 1 BTC.

Sample SELL request:

```json
{
  "Order": {
    "Type": "Sell",
    "Amount": 3
  },
  "ExchangeBalances": [
    {
      "BtcBalance": 1.5,
      "Id": "PipiExchange"
    },
	{
      "BtcBalance": 1.5,
      "Id": "MelkijadExchange"
    }
  ]
}
```

Sample BUY request:

```json
{
  "Order": {
    "Type": "Buy",
    "Amount": 10
  },
  "ExchangeBalances": [
    {
      "EurBalance": 5000,
      "Id": "PipiExchange"
    },
	{
      "EurBalance": 5000,
      "Id": "MelkijadExchange"
    }
  ]
}
```

## Building and running Docker

To build and push a Docker image to the Docker Hub:

1) `docker build -t akovac35/hedger .`
2) `docker push akovac35/hedger`
3) `docker tag akovac35/hedger akovac35/hedger:v1.0.0`
4) `docker push akovac35/hedger:v1.0.0`

To run from Docker Hub, execute `docker run -d -p 5000:5000 akovac35/hedger` and navigate to `http://localhost:5000/swagger/index.html`.

Basic troubleshooting can be performed by examining container logs, which the API is configured to emit.

## TODO and FYI

This sample is just a demonstration of what is possible with a limited intended functionality scope, but an explicit TODO and FYI list is still in order:

* In the real world, there would be countless of corner cases - request failures, transaction issues, timed-out order books, performance considerations, security considerations, and so on and on,
* if the API is planned to grow in complexity, consider using fluent validation or something similar to cover data validation well, use DTOs where meaningfull, etc.,
* log files are now stored directly in the build folder. This will result in Docker problems. The default configuration can be provided to solve this problem, including dynamic log levels etc.,
* the applicaton is loading sample exchange platform order books in cache and then using provided exchange platform balances to create relevant pairs per request,
* appsettings.json should be split per environment - dev, test, stage, etc. Environment variables should be provided for Docker image. Stop emitting traces to those logs,
* same as above for log settings file serilog.json,
* security is not implemented,
* provide better defaults for the Docker container,
* link git with Docker for devops, use proper git workflows,
* configure Docker SSL certificate and a valid SSL configuration for the API,
* ...

## Author

Aleksander Kovač

## License
[Apache-2.0](LICENSE)
