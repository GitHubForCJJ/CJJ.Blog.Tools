using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CJJ.Blog.Service.Models.Data;
using HtmlAgilityPack;
using CJJ.Blog.NetWork.WcfHelper;
using FastDev.Log;

namespace BaseWork
{
    public class NewsJob : WorkBase
    {
        public override void RunWork()
        {
            try
            {
                HttpHelper httpHelper = new HttpHelper();
                HttpItem httpitem = new HttpItem()
                {
                    URL = "https://www.oschina.net/news/widgets/_news_index_all_list?p=1&type=ajax", //URL     必需项  
                    Method = "GET", //URL     可选项 默认为Get  
                    IsToLower = false, //得到的HTML代码是否转成小写     可选项默认转小写  
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36", //用户的浏览器类型，版本，操作系统     可选项有默认值  
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3", //    可选项有默认值  
                    ContentType = "text/html", //返回类型    可选项有默认值  
                    ResultType = ResultType.String, //返回数据类型，是Byte还是String  
                };
                HttpResult httpResult = httpHelper.GetHtml(httpitem);
                var html = httpResult.Html;
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var nodel = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='ui very relaxed items list-container news-list-container']");
                var nodellist = nodel.SelectNodes("//h3[@class='header']");
                var hotnewslist = new List<HotNew>();

                #region 获取上次抓取的hotnew，用于去重
                var zuotiandata = BlogHelper.GetJsonListPage_HotNew(1, 20, "CreateTime desc", new Dictionary<string, object>()
                {
                    { nameof(HotNew.IsDeleted),0 }
                });
                var titlelist = new List<string>();
                titlelist = zuotiandata?.data?.Select(x => x.Title).ToList();
                var time = DateTime.Now;
                #endregion

                foreach (HtmlNode item in nodellist)
                {

                    var h3 = item.SelectSingleNode("a");
                    var url = h3.Attributes["href"].Value;
                    var title = h3.Attributes["title"].Value;
                    //去重
                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title) && !titlelist.Contains(title))
                    {
                        var news = new HotNew();
                        news.Url = url;
                        news.Title = title;
                        news.CreateTime = time;
                        news.UpdateTime = time;
                        news.CreateUserId = "1";
                        news.CreateUserName = "system";
                        hotnewslist.Add(news);
                    }
                }
                if (hotnewslist.Count > 0)
                {
                    var res=BlogHelper.AddByEntitys_HotNew(hotnewslist, new CJJ.Blog.Service.Models.View.OpertionUser()
                    {
                        UserId = "1",
                        UserClientIp = "0.0.0.1",
                        UserName = "system"
                    });
                    if (res.IsSucceed)
                    {
                        Console.WriteLine($"抓取{hotnewslist.Count}条数据成功，{DateTime.Now}");
                    }
                    else
                    {
                        Console.WriteLine($"抓取{hotnewslist.Count}条数据-插入数据失败，{DateTime.Now}");
                    }

                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex, "抓取数据出错");   
            }
      

        }
    }
}
