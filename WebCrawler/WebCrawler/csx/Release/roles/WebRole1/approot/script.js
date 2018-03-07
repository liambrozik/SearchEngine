var refresh = 5000;
var crawling = true;
GetStats();

function sleep(time) {
    return new Promise((resolve) => setTimeout(resolve, time));
}
async function GetStats() {
    while (crawling) {
        await sleep(refresh).then(() => {
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/GetStats",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    console.log("GetStats()");
                    console.log(eval(msg));
                    $("#total-links").html("<center class='padabove'><h3>" + msg["d"][0] + " links</h3></center>");
                    $("#cpu-body").text(msg["d"][1]);
                    lineChart.data.labels.push("");
                    if (cpuStack.length >= 26) {
                        var old = cpuStack.pop();
                    }
                    cpuStack.push(parseInt(msg["d"][1]));
                    lineChart.update();
                    $("#ram-body").text(msg["d"][2]);
                    if (ramStack.length >= 26) {
                        var old2 = ramStack.pop();
                    }
                    ramStack.push(parseInt(msg["d"][2]));
                    lineChart2.update();
                    $("#crawler-status").text(msg["d"][3]);
                },
                error: function (msg) {
                    console.log(eval(msg));
                }
            });
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/GetErrors",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    console.log("GetErrors()");
                    console.log(eval(msg));
                    $(".err").remove();
                    for (var i = 0; i < msg["d"].length - 1; i++) {
                        $("#error-body").append("<tr class='alert alert-danger err' role='alert'><td class='err-msg'><div>" + msg["d"][i] + "</div></td><td class='err-link'>" + msg["d"][i + 1] + "</td></tr>");
                    }
                },
                error: function (msg) {
                    console.log(eval(msg));
                }
            });
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/GetLastTenLinks",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    console.log("GetLastTen()");
                    console.log(eval(msg));
                    for (var i = 0; i < msg["d"].length; i++) {
                        $("#link" + i).html("<a href='" + msg["d"][i] + "' >" + msg["d"][i] + "</a>");
                    }
                },
                error: function (msg) {
                    console.log(eval(msg));
                }
            });
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/GetQueueCount",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    console.log("GetQueueCount()");
                    console.log(eval(msg));
                    $("#queue-links").html("<center class='padabove'><h3>" + msg["d"] + " links</h3></center>");
                },
                error: function (msg) {
                    console.log(eval(msg));
                }
            });
            lineChart.update();
            lineChart2.update();
        });

    }
    $("#crawler-status").text("Stopped");
}

function Badge(message) {
    $("#badge-message").text(message);
    $("#badge").addClass("show");
    setTimeout(function () {
        $("#badge").removeClass("show");
    }, 5000);
}

$("#refresh-rate-input").on("change", function () {
    if ($("#refresh-rate-input").val() >= 3) {
        refresh = $("#refresh-rate-input").val() * 1000;
        Badge("Refresh rate set to " + $("#refresh-rate-input").val() + " seconds");
    } else {
        refresh = 3000;
        Badge("Refresh rate set to maximum of 3 seconds");
    }

});

$("#ClearQueueConfirm").on("click", function () {
    var pw = $("#ClearQueuePassword").val();
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearQueue?password=",
        data: JSON.stringify({ password: pw }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log(eval(msg));
            Badge(msg["d"]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
});
$("#ClearEverythingConfirm").on("click", function () {
    var pw = $("#ClearEverythingPassword").val();
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearQueue?password=",
        data: JSON.stringify({ password: pw }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log(eval(msg));
            Badge(msg["d"]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearTable?password=",
        data: JSON.stringify({ password: pw }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log(eval(msg));
            Badge(msg["d"]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/StopCrawler",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            Badge(msg["d"]);
            $("crawler-status").text("Stopped");
            $("#stop-crawler").text("Start");
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearErrors",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            Badge(msg["d"]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
});

$("#ClearTableConfirm").on("click", function () {
    var pw = $("#ClearTablePassword").val();
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearTable?password=",
        data: JSON.stringify({ password: pw }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log(eval(msg));
            Badge(msg["d"]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
});

$("#title-search-submit").on("click", function () {
    var link = $("#title-search").val();
    setTimeout(function () {
        $("title-search").val("Searching...");
    }, 50);
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/GetTitle?q=",
        data: JSON.stringify({ q: link }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log("SEARCH TITLES!");
            console.log(eval(msg));
            $("#search-result").show();
            $("#search-result-title").html("<b><a href='" + link + "'>" + msg["d"][0] + "</a></b>");
            $("#search-result-date").text(msg["d"][1]);
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });
});

$("#CreateNewCrawler").on("click", function () {
    var url = $("#new-crawler-url").val();
    var name = $("#new-crawler-name").val();
    var pw = $("#CreateCrawlerPassword").val();
    $.ajax({
        type: "POST",
        url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/ClearQueue?password=",
        data: JSON.stringify({ password: pw }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            Badge(msg["d"]);
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/NewCrawler?u=",
                data: JSON.stringify({ u: url }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                },
                error: function (msg) {
                    Badge("Crawler failed to initialize");
                    console.log(eval(msg));
                }
            });
            $.ajax({
                type: "POST",
                url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/StartCrawler",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    Badge(msg["d"]);
                    crawling = true;
                    $("#crawler-row").show();
                    $("#crawler-name").text(name);
                    $("#crawler-status").text("Loading");
                    $("#crawler-link").append("<a href='" + url + "' >" + url.replace(/http:\/\/.+?\./, '') + "</a>");
                },
                error: function (msg) {
                    console.log(eval(msg));
                }
            });
        },
        error: function (msg) {
            console.log(eval(msg));
        }
    });


});

$("#stop-crawler").on("click", function () {
    if ($("#stop-crawler").text() === "Stop") {
        $.ajax({
            type: "POST",
            url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/StopCrawler",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                Badge(msg["d"]);
                $("crawler-status").text("Stopped");
                $("#stop-crawler").text("Start");
            },
            error: function (msg) {
                console.log(eval(msg));
            }
        });

    } else {
        $.ajax({
            type: "POST",
            url: "http://crawlerstorageacc.cloudapp.net/Admin.asmx/StartCrawler",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                Badge(msg["d"]);
                crawling = true;
                $("#crawler-status").text("Loading");
                $("#stop-crawler").text("Stop");
            },
            error: function (msg) {
                console.log(eval(msg));
            }
        });
    }


});

//CHARTJS --------------- 
var ctx = $("#line-chart1");
var ctz = $("#line-chart2");
var cpuStack = [0, 100];
var ramStack = [0, 800];
ctx.strokeStyle = 'red';
Chart.defaults.global.defaultFontColor = "#000000";
Chart.defaults.global.defaultColor = "#006dcc";
var lineChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: ["", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Time"],
        datasets: [
            {
                label: "",
                data: cpuStack,
                fill: false,
                borderColor: "#006dcc"
            }
        ]
    },
    options: {
        animation: false,
        scales: {
            xAxes: [{ gridLines: { color: "#ffffff" } }],
            yAxes: [{ gridLines: { color: "#ffffff" } }]
        }
    }
});
var lineChart2 = new Chart(ctz, {
    type: 'line',
    data: {
        labels: ["", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Time"],
        datasets: [
            {
                label: "",
                data: ramStack,
                fill: false,
                borderColor: "#006dcc"
            }
        ]
    },
    options: {
        animation: false,
        scales: {
            xAxes: [{ gridLines: { color: "#ffffff" } }],
            yAxes: [{ gridLines: { color: "#ffffff" } }]
        }
    }
});

$(document).ready(function () {
    $("#settings").hide();
    $("#search-bg").hide();
    $("#search-mod").hide();
    $("#search-result").hide();
    $("#settings").addClass("settings-animation-out");
});
$("#settings-open").on("click", function () {
    $("#settings").show();
    setTimeout(function () {
        $("#settings").removeClass("settings-animation-out");
        $("#settings").addClass("settings-animation-in");
    }, 10);
});

$("#search-open").on("click", function () {
    $("#search-bg").show();
    setTimeout(function () {
        $("#search-bg").removeClass("search-bg-animation-out");
        $("#search-bg").addClass("search-bg-animation-in");
    }, 50);
    setTimeout(function () {
        $("#search-mod").show();
    }, 250);
    setTimeout(function () {
        $("#search-mod").removeClass("search-mod-animation-out");
        $("#search-mod").addClass("search-mod-animation-in");
    }, 300);

});

$("#search-close").on("click", function () {
    $("#search-mod").removeClass("search-mod-animation-in");
    $("#search-mod").addClass("search-mod-animation-out");
    setTimeout(function () {
        $("#search-bg").removeClass("search-bg-animation-in");
        $("#search-bg").addClass("search-bg-animation-out");
    }, 100);
    setTimeout(function () {
        $("#search-mod").hide();
    }, 250);
    setTimeout(function () {
        $("search-bg").hide();
    }, 400);

});

$("#settings-close").on("click", function () {
    $("#settings").removeClass("settings-animation-in");
    $("#settings").addClass("settings-animation-out");
    setTimeout(function () {
        $("#settings").hide();
    }, 500);
});

$("#theme-form").on("change", function () {
    if ($("#option-dark").is(":checked")) {
        $("body").addClass("dark");
        $(".mod-header").addClass("dark");
        $(".mod-body").addClass("dark");
        $("#settings").addClass("dark");
        $("#search-bg").addClass("dark");
        $(".refresh-rate-input").addClass("dark");
    }
    if ($("#option-light").is(":checked")) {
        $("body").removeClass("dark");
        $(".mod-header").removeClass("dark");
        $(".mod-body").removeClass("dark");
        $("#settings").removeClass("dark");
        $("#search-bg").removeClass("dark");
        $(".refresh-rate-input").removeClass("dark");
    }
});
