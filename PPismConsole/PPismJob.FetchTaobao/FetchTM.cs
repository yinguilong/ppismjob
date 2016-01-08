/************************************************************************************
 * Copyright (c) 2015Microsoft All Rights Reserved.
 * CLR版本： 4.5
 *命名空间：PPismJob.FetchTaobao
 *文件名：  FetchTM
 *版本号：  V1.0.0.0
 *创建人：  yinguilong
 *创建时间：12/22/2015 6:15:13 PM
 *描述：
 *
/************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using PPism.Infrastructure;
using PPism.Domain;
using PPism.Domain.Repositories;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace PPismJob.FetchTaobao
{
    [DisallowConcurrentExecution]
    public class FetchTM : IJob
    {
        private readonly IPPismItemRepository _ppismItemRepository = ServiceLocator.Instance.GetService<IPPismItemRepository>();
        private readonly IRepositoryContext _repositoryContext = ServiceLocator.Instance.GetService<IRepositoryContext>();
        private readonly IPriceItemRepository _priceItemRepository = ServiceLocator.Instance.GetService<IPriceItemRepository>();
        public virtual void Execute(IJobExecutionContext context)
        {
            var dateTimeMin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//当天零点
            var list = _ppismItemRepository.GetAll(x => (x.ItemSource == PPism.Model.Enum.DictPPItemSource.天猫 || x.ItemSource == PPism.Model.Enum.DictPPItemSource.淘宝) && (!x.LastListenTime.HasValue || x.LastListenTime.Value < dateTimeMin)).ToList();
            for (int i = 0, length = list.Count; i < length; i++)
            {
                var item = list[i];
                Process p = new Process();
                try
                {
                    var environment = Environment.CurrentDirectory;

                    p.StartInfo.FileName = environment + "\\phantomjs\\bin\\phantomjs.exe";

                    p.StartInfo.WorkingDirectory = environment + "\\phantomjs\\bin\\";

                    string strArg = @"{0}\phantomjs\bin\tmallsavehtml.js ""{1}"" ""{0}\{2}""";
                    p.StartInfo.Arguments = string.Format(strArg, environment, item.ListenUrl, "\\phantomjs\\htmltmall\\" + item.Id.ToString());

                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    if (!p.Start())
                        throw new Exception("无法Headless浏览器.");
                    Thread.Sleep(2 * 1000);
                    string fileUrl = string.Format(@"{0}\phantomjs\htmltmall\{1}.html", environment, item.Id.ToString());
                    var fileInfo = new FileInfo(fileUrl);
                    if (!fileInfo.Exists || fileInfo.LastWriteTime < DateTime.Now.AddHours(-1))
                        Thread.Sleep(5 * 1000);//如果还没有返回就再等5秒
                    if (File.Exists(fileUrl))
                    {
                        var htmldocument = new HtmlAgilityPack.HtmlDocument();
                        htmldocument.Load(fileUrl);
                        var strHtml = htmldocument.DocumentNode.InnerHtml.ToString();
                        string reg = @"id=""J_PromoPrice""[\s\S]+?class=""tm-price"">(?<price>[^<]+)[\s\S]+?<img\s+id=""J_ImgBooth""[\s\S]+?src=""(?<imgUrl>[^""]+)";
                        if (item.ItemSource == PPism.Model.Enum.DictPPItemSource.淘宝)//淘宝和天猫正则不一样
                            reg = @"id=""J_PromoPriceNum""[\s\S]+?class=""tb-rmb-num"">(?<price>[^<]+)[\s\S]+?<img\s+id=""J_ImgBooth""[\s\S]+?src=""(?<imgUrl>[^""]+)";
                        var groups = Regex.Match(strHtml, reg).Groups;
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
                catch (Exception ex)
                {

                    throw ex;
                }
                finally
                {
                    p.Dispose();
                }
            }
            _repositoryContext.Commit();
        }
    }
}

