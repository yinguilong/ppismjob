/************************************************************************************
 * Copyright (c) 2015Microsoft All Rights Reserved.
 * CLR版本： 4.5
 *命名空间：PPismJob.FetchJD
 *文件名：  FetchJDSelf
 *版本号：  V1.0.0.0
 *创建人：  yinguilong
 *创建时间：12/23/2015 5:50:37 PM
 *描述：
 *
/************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using PPism.Domain.Repositories;
using PPism.Infrastructure;
using PPism.Domain;
using xNet;
using System.Text.RegularExpressions;
using PPism.Model.Enum;
using PPism.Domain.Model;
namespace PPismJob.FetchJD
{
    public class FetchJDSelf : IJob
    {
        private readonly IPPismItemRepository _ppismItemRepository = ServiceLocator.Instance.GetService<IPPismItemRepository>();
        private readonly IRepositoryContext _repositoryContext = ServiceLocator.Instance.GetService<IRepositoryContext>();
        private readonly IPriceItemRepository _priceItemRepository = ServiceLocator.Instance.GetService<IPriceItemRepository>();
        public virtual void Execute(IJobExecutionContext context)
        {
            var dateTimeMin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//当天零点
            var list = _ppismItemRepository.GetAll(x => x.ItemSource == PPism.Model.Enum.DictPPItemSource.京东 && (!x.LastListenTime.HasValue || x.LastListenTime.Value < dateTimeMin)).ToList();
            //

            for (int i = 0, length = list.Count; i < length; i++)
            {
                var item = list[i];
                using (var req = new xNet.Net.HttpRequest())
                {
                    req.UserAgent = xNet.Net.HttpHelper.FirefoxUserAgent();
                    //http://p.3.cn/prices/get?callback=cnp&type=1&area=1_72_2799&pdtk=&pduid=2002986638&pdpin=&pdbp=0&skuid=J_540462
                    var strHtml = req.Get(item.ListenUrl).ToString();
                    var mUrl = Regex.Match(strHtml, @"content=""format\s*=\s*html5;\s*url\s*=(?<mUrl>[^""]+)").Groups["mUrl"].Value.Trim();
                    if (!string.IsNullOrEmpty(mUrl))
                    {
                        req.CharacterSet = System.Text.Encoding.GetEncoding("utf-8");
                        var strMHtml = req.Get("http:" + mUrl).ToString();
                        string reg = @"id=""goods-img-box""[\s\S]+?<img[\s\S]+?src=""(?<imgUrl>[^""]+)[\s\S]+?prod-price"">\s*?<span>[\s\S]*?</span>(?<price>[^<]+)";
                        var groups = Regex.Match(strMHtml, reg).Groups;
                        var price = groups["price"].Value.Trim().ToDecimal(0);
                        var imgUrl = groups["imgUrl"].Value.Trim();
                        if (price > 0 && !string.IsNullOrEmpty(imgUrl))
                        {
                            var ppismItemBll = new PPismJob.Common.PPismItemBll();
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

