using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DanDanPlayResourceService.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Type = DanDanPlayResourceService.Models.Type;

namespace DanDanPlayResourceService.Controllers
{
    [ApiController]
    [Route("/")]
    public class DhmyResourceController : ControllerBase
    {
        private const int UnknownSubgroupId = -1;
        private const string UnknownSubgroupName = "未知字幕组";
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _dmhyBaseUrl;
        private readonly string _dmhyListUrl;
        private readonly string _dmhyTypeAndSubgroupUrl;
        private readonly ILogger<DhmyResourceController> _logger;

        public DhmyResourceController(ILogger<DhmyResourceController> logger, IConfiguration configuration,
            IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _dmhyBaseUrl = _configuration.GetValue<string>("DMHYBaseUri");
            _dmhyListUrl = _configuration.GetValue<string>("DMHYListUri");
            _dmhyTypeAndSubgroupUrl = _configuration.GetValue<string>("DMHYTypeAndSubgroupUri");
        }

        [HttpGet("subgroup")]
        public async Task<SubgroupWrap> GetSubgroups()
        {
            var client = _clientFactory.CreateClient();

            var response = await client.GetAsync(_dmhyTypeAndSubgroupUrl);
            var responseString = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseString);
            var nodes = htmlDoc.DocumentNode
                .SelectNodes("//*[@id=\"AdvSearchTeam\"]/option");
            var subgroupWrap = new SubgroupWrap(new List<Subgroup>());
            foreach (var node in nodes)
                subgroupWrap.Subgroups.Add(new Subgroup(
                    int.Parse(node.Attributes["value"].Value),
                    node.InnerText
                ));

            subgroupWrap.Subgroups.Add(new Subgroup(
                UnknownSubgroupId,
                UnknownSubgroupName
            ));
            return subgroupWrap;
        }

        [HttpGet("type")]
        public async Task<TypeWrap> GetTypes()
        {
            var client = _clientFactory.CreateClient();

            var response = await client.GetAsync(_dmhyTypeAndSubgroupUrl);
            var responseString = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseString);
            var nodes = htmlDoc.DocumentNode
                .SelectNodes("//*[@id=\"AdvSearchSort\"]/option");
            var typeWrap = new TypeWrap(new List<Type>());
            foreach (var node in nodes)
                typeWrap.Types.Add(new Type
                {
                    Id = int.Parse(node.Attributes["value"].Value),
                    Name = node.InnerText
                });

            return typeWrap;
        }

        [HttpGet("list")]
        public async Task<SearchResult> GetList(string keyword, int subgroup = 0, int type = 0, string? r = null)
        {
            var client = _clientFactory.CreateClient();
            var requestUri = string.Format(_dmhyListUrl, keyword, type, subgroup);
            var response = await client.GetAsync(requestUri);
            var responseString = await response.Content.ReadAsStringAsync();
            // _logger.LogDebug("{ResponseString}", responseString);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseString);
            htmlDoc.OptionEmptyCollection = true;
            var nodes = htmlDoc.DocumentNode
                .SelectNodes("//*[@id=\"topic_list\"]/tbody/tr");
            var nextPageNode = htmlDoc.DocumentNode
                .SelectSingleNode("//*[contains(@class,'nav_title')]//a[contains(text(),'下一頁')]");
            var searchResult = new SearchResult(new List<Resource>())
            {
                HasMore = nextPageNode != null
            };
            foreach (var node in nodes)
            {
                var td0 = node.SelectSingleNode("td[1]");
                var td1 = node.SelectSingleNode("td[2]");
                var td2 = node.SelectSingleNode("td[3]");
                var td3 = node.SelectSingleNode("td[4]");
                var td4 = node.SelectSingleNode("td[5]");
                var c1 = td2.SelectNodes(".//a").Count;
                var td1_a0 = td1.SelectSingleNode(".//a[1]");
                var td2_a0 = td2.SelectSingleNode(".//a[1]");
                var td2_a_last = td2.SelectSingleNode("(.//a)[last()]");
                var td3_a0 = td3.SelectSingleNode(".//a[1]");
                searchResult.Resources.Add(new Resource
                {
                    Title = td2_a_last.InnerText.Trim(),
                    TypeId = int.Parse(td1_a0.GetAttributeValue("href", "").Replace("/topics/list/sort_id/", "")),
                    TypeName = td1_a0.InnerText.Trim(),
                    SubgroupId = c1 != 2
                        ? UnknownSubgroupId
                        : int.Parse(td2_a0.GetAttributeValue("href", "").Replace("/topics/list/team_id/", "")),
                    SubgroupName = c1 != 2 ? UnknownSubgroupName : td2_a0.InnerText.Trim(),
                    Magnet = td3_a0.GetAttributeValue("href", ""),
                    PageUrl = _dmhyBaseUrl + td2_a_last.GetAttributeValue("href", ""),
                    FileSize = td4.InnerText.Trim(),
                    PublishDate = DateTime.Parse(td0.SelectSingleNode(".//span[1]").InnerText.Trim())
                        .ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            return searchResult;
        }
    }
}