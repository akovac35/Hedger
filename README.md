# Cryptocurrency hedger API sample

This project contains a cryptocurrency hedger API sample which, in short, distributes a single cryptocurrency order between different exchange platform order books while also accounting for a particular exchange platform balance.

## Executing tests

Review tests, then execute ```dotnet test``` in the project folder.

## Testing API locally

Navigate to ../Hedger.Api, execute ```dotnet test``` and navigate to `https://localhost:5001/swagger/index.html`.

Sample SELL request:

```json
{
  "OrderInstance": {
    "Type": "Sell",
    "Amount": 1
  },
  "ExchangeBalances": [
    {
      "InBtc": 0.5,
      "Id": "PipiExchange"
    },
	{
      "InBtc": 0.5,
      "Id": "MelkijadExchange"
    }
  ]
}
```

Sample BUY request:

```json
{
  "OrderInstance": {
    "Type": "Buy",
    "Amount": 10
  },
  "ExchangeBalances": [
    {
      "InEur": 5000,
      "Id": "PipiExchange"
    },
	{
      "InEur": 5000,
      "Id": "MelkijadExchange"
    }
  ]
}
```

## Building docker

