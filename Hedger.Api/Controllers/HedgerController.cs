using com.github.akovac35.Logging;
using Hedger.Api.Model;
using Hedger.Core;
using Hedger.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Hedger.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class HedgerController : ControllerBase
    {
        public HedgerController(HedgerService hedgerService, IMemoryCache memoryCache, ILogger<HedgerController>? logger = null)
        {
            if (logger != null) _logger = logger;
            HedgerServiceInstance = hedgerService ?? throw new ArgumentNullException(nameof(hedgerService));
            MemoryCacheInstance = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        private readonly ILogger _logger = NullLogger.Instance;

        protected HedgerService HedgerServiceInstance { get; }
        public IMemoryCache MemoryCacheInstance { get; }

        /// <summary>
        /// Prepares an order plan based on the provided order and order books from various cryptocurrency exchanges and their balances. 
        /// </summary>
        /// <returns>An order plan.</returns>
        /// <response code="200">An order plan.</response>
        [HttpPost(nameof(PrepareOrderPlan))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<PrepareOrderPlanResponse> PrepareOrderPlan(PrepareOrderPlanRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (request.Order == null) throw new ArgumentNullException(nameof(request.Order));
                if (request.ExchangeBalances == null) throw new ArgumentNullException(nameof(request.ExchangeBalances));

                var response = new PrepareOrderPlanResponse();
                var cachedOrderBooks = MemoryCacheInstance.Get<List<OrderBook>>(ApiConstants.ExchangeOrderBooksCacheKey);

                response.OrderPlan = HedgerServiceInstance.PrepareOrderPlan(request.Order, cachedOrderBooks, request.ExchangeBalances);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Here(l => l.LogError(ex, ex.Message));
                return Problem(detail: ex.StackTrace, title: ex.Message, statusCode: 500);
            }
        }
    }
}
