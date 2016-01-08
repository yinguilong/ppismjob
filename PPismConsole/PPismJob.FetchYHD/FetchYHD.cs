/************************************************************************************
 * Copyright (c) 2015Microsoft All Rights Reserved.
 * CLR版本： 4.5
 *命名空间：PPismJob.FetchYHD
 *文件名：  FetchYHD
 *版本号：  V1.0.0.0
 *创建人：  yinguilong
 *创建时间：1/6/2016 6:10:42 PM
 *描述：
 *
/************************************************************************************/
using PPism.Domain.Repositories;
using PPism.Infrastructure;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PPismJob.FetchYHD
{
    public class FetchYHD : IJob
    {
        private readonly IPPismItemRepository _ppismItemRepository = ServiceLocator.Instance.GetService<IPPismItemRepository>();
        private readonly IRepositoryContext _repositoryContext = ServiceLocator.Instance.GetService<IRepositoryContext>();
        private readonly IPriceItemRepository _priceItemRepository = ServiceLocator.Instance.GetService<IPriceItemRepository>();
        public virtual void Execute(IJobExecutionContext context)
        {
            var dateTimeMin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//当天零点
            var list = _ppismItemRepository.GetAll(x => (x.ItemSource == PPism.Model.Enum.DictPPItemSource.一号店) && (!x.LastListenTime.HasValue || x.LastListenTime.Value < dateTimeMin)).ToList();
            var ppismItemBll = new PPismJob.Common.PPismItemBll();
            for (int i = 0, length = list.Count; i < length; i++)
            {
                var item = list[i];
                string mUrl = item.ListenUrl;
                if (!ppismItemBll.CheckIsMUrl(item.ListenUrl))
                {
                    var htmlWeb = new HtmlAgilityPack.HtmlWeb();
                    var strHtml = htmlWeb.Load(item.ListenUrl).DocumentNode.InnerHtml.ToString();
                     mUrl = Regex.Match(strHtml, @"name=""h5""\scontent='(?<mUrl>[^']+)'").Groups["mUrl"].Value.Trim();
                }
                if (!string.IsNullOrEmpty(mUrl))
                {
                    using (var req = new xNet.Net.HttpRequest())
                    {
                        req.UserAgent = xNet.Net.HttpHelper.FirefoxUserAgent();
                        //http://p.3.cn/prices/get?callback=cnp&type=1&area=1_72_2799&pdtk=&pduid=2002986638&pdpin=&pdbp=0&skuid=J_540462
                        req.CharacterSet = System.Text.Encoding.GetEncoding("utf-8");
                        var strMHtml = req.Get(mUrl).ToString();
                        string reg = @"class=""swipeSlide_detail"">[\s\S]+?<img[\s]src=""(?<imgUrl>[^""]+)[\s\S]+?id=""current_price""[\s\S]+?class=""pd_product-price-num"">(?<price>[^<]+)";
                        var groups = Regex.Match(strMHtml, reg).Groups;
                        var price = groups["price"].Value.Trim().ToDecimal(0);
                        var imgUrl = groups["imgUrl"].Value.Trim();
                        if (price > 0 && !string.IsNullOrEmpty(imgUrl))
                        {
                         
                            var priceItem = ppismItemBll.GetPriceItem(item, price, imgUrl);
                            _priceItemRepository.Add(priceItem);
                            _ppismItemRepository.Update(item);
                        }
                    }
                }
            }
            _repositoryContext.Commit();
        }
    }
}

