/************************************************************************************
 * Copyright (c) 2015Microsoft All Rights Reserved.
 * CLR版本： 4.5
 *命名空间：PPismJob.Common
 *文件名：  PPismItemBll
 *版本号：  V1.0.0.0
 *创建人：  yinguilong
 *创建时间：1/4/2016 1:38:46 PM
 *描述：
 *
/************************************************************************************/
using PPism.Domain.Repositories;
using PPism.Infrastructure;
using PPism.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPism.Domain.Model;
using System.Text.RegularExpressions;

namespace PPismJob.Common
{
    public class PPismItemBll
    {
        private readonly IPPismItemRepository _ppismItemRepository = ServiceLocator.Instance.GetService<IPPismItemRepository>();
        private readonly IRepositoryContext _repositoryContext = ServiceLocator.Instance.GetService<IRepositoryContext>();
        private readonly IPriceItemRepository _priceItemRepository = ServiceLocator.Instance.GetService<IPriceItemRepository>();
        public PPism.Model.Enum.DictPriceTrend GetPriceTrend(PPism.Domain.Model.PPismItem pps, decimal currentPrice)
        {
            var prices = _priceItemRepository.GetAll(
                x => x.PPismItem.Id == pps.Id,
                x => x.UpdateTime,
                System.Data.SqlClient.SortOrder.Descending);
            if (prices == null || !prices.Any()) return DictPriceTrend.无变化;
            var priceFirst = prices.FirstOrDefault();

            if (currentPrice > priceFirst.Price) return DictPriceTrend.涨价;
            else if (currentPrice == priceFirst.Price) return DictPriceTrend.无变化;
            else return DictPriceTrend.降价;
        }
        public PPism.Domain.Model.PriceItem GetPriceItem(PPismItem item, decimal price, string imgUrl)
        {
            item.CurrentPrice = price;
            if (imgUrl.StartsWith("//"))
            {
                imgUrl = "http:" + imgUrl;
            }
            item.ItemImage = imgUrl;
            item.LastListenTime = DateTime.Now;
            item.Trend = GetPriceTrend(item, price);//获取价格变化趋势
            var priceItem = new PPism.Domain.Model.PriceItem()
            {
                PPismItem = item,
                ActivityPrice = price,
                Price = price,
                UpdateTime = DateTime.Now
            };
            return priceItem;
        }
        public bool CheckIsMUrl(string url)
        {
            string reg = @"\.m\.[\S]+?\.com";
            if (Regex.IsMatch(url, reg))
                return true;
            return false;
        }
    }
}

