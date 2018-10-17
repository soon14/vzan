var httphelper = require("./helper/httphelper");
var schedule = require("node-schedule");  
function addmessage(){
    httphelper.post("http://testwtapi.vzan.com/apiim/AddMessageRecord").then(function(data){
    })
}
var rule1     = new schedule.RecurrenceRule();  
var times1    = [1];  
rule1.second  = times1;  
schedule.scheduleJob(rule1, function(){  
    addmessage();  
}); 
