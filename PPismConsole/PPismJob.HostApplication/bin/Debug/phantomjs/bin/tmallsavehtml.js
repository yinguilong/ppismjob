var page = require('webpage').create(),
    system = require('system'), address, name;
var fs = require("fs");
console.log('start');
if (system.args.length === 1) {
    console.log('Usage: loadspeed.js <some URL>');
    phantom.exit();
}

address = system.args[1];
name = system.args[2];
page.open(address, function (status) {
    if (status !== 'success') {
        console.log('FAIL to load the address');
    } else {
        var text = page.evaluate(function () {
            return document.body.innerHTML;
        });
        fs.write(name + '.html', text, "w");
    }
    console.log('end');
    phantom.exit();
});