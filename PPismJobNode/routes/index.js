/// <reference path='../typings/tsd.d.ts' />
var express = require('express');
var webpage = require('webpage');
var router = express.Router();
/* GET home page. */
router.get('/', function (req, res, next) {
    var page = webpage.create();
    page.open("https://chaoshi.detail.tmall.com/item.htm?spm=1.1.0.0.EfvkwZ&id=45801661329&rewcatid=50514010&skuId=87920082398", function () {
        var text = page.evaluate(function () {
            return document.body.innerHTML;
        });
        phantom.exit();
        res.send(text);
    });
});
module.exports = router;
//# sourceMappingURL=index.js.map