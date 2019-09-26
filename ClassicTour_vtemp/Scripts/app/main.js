var app = angular.module('CTour', ['ngMask', 'angucomplete-alt', 'angularFileUpload']);

var webaddress = "http://localhost:7938";

app.controller('HomeCtrl', function ($scope, $http) {

    //Setting intro params
    $scope.tourlist = [];
    $scope.details = [];

    $scope.step_a = true;
    $scope.step_b = false;
    $scope.step_c = false;

    //Getting Tours
    $http({
        method: "GET",
        url: webaddress + "/Home/Get_Tours"
    }).then(function mySuccess(response) {
        $scope.tourlist = response.data;
    }, function myError(response) {
        alert(response.statusText);
    });

    $scope.nextstep = function () {
        if ($scope.step_a) {
            $scope.step_a = false;
            $scope.step_b = true;
            $scope.breadcrumbs = ["صفحه اصلی", "جزئیات تور"];
        } else
            if ($scope.step_b) {
                $scope.step_b = false;
                $scope.step_c = true;
                $scope.breadcrumbs = ["صفحه اصلی", "جزئیات تور", "پرداخت"];
            }
    };
    $scope.previousstep = function () {
        if ($scope.step_b) {
            $scope.step_b = false;
            $scope.step_a = true;
        } else
            if ($scope.step_c) {
                $scope.step_c = false;
                $scope.step_b = true;
            }
    };

    $scope.setdetails = function (id) {
        $scope.details = $scope.tourlist[id];
        $scope.nextstep();
    };

    $scope.fullName = function () {
        return $scope.firstName + " " + $scope.lastName;
    };

    $scope.buy = function () {
        grecaptcha.ready(function () {
            grecaptcha.execute('6Le8AnkUAAAAAEBSbeuosz0mRshE3WwbSXCr-108', { action: 'reqbuy' })
                .then(function (token) {
                    $http({
                        method: "GET",
                        url: webaddress + "/Home/buy/?token=" + token + "&id=" + $scope.details.id + "&count=" + $scope.count
                    }).then(function mySuccess(response) {
                        //alert(response.data);
                        window.location.replace(response.data);
                    }, function myError(response) {
                        alert(response.statusText);
                    });
                });
        });
    };

});

app.controller('ManagementCtrl', function ($scope, $http, FileUploader) {

    //_______________________________________________________________________________________________________________| .:: توابع کوچک ::.

    function showmsg(inputmsg, inputcolor) {
        $scope.AlertVisibility = true;
        $scope.AlertMessage = inputmsg;
        $scope.AlertColor = inputcolor;
    }//نمایش اخطار
    $scope.hidemsg = function () {
        $scope.AlertVisibility = false;
    };//مخفی کردن اخطار
    $scope.makepassword = function () {
        $http({
            method: "GET",            url: "https://www.random.org/strings/?num=1&len=8&digits=on&loweralpha=on&unique=on&format=plain&rnd=new"
        }).then(function mySuccess(response) {
            $scope.pass_word = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });

    };//ساختن گذرواژه
    $scope.makepassword = function () {
        $http({
            method: "GET",            url: "https://www.random.org/strings/?num=1&len=8&digits=on&loweralpha=on&unique=on&format=plain&rnd=new"
        }).then(function mySuccess(response) {
            $scope.pass_word = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });

    };//ساختن گذرواژه
    $scope.makepassword = function () {
        $http({
            method: "GET",            url: "https://www.random.org/strings/?num=1&len=8&digits=on&loweralpha=on&unique=on&format=plain&rnd=new"
        }).then(function mySuccess(response) {
            $scope.pass_word = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });

    };//ساختن گذرواژه

    //_______________________________________________________________________________________________________________| .:: توابع مشتریان ::.

    $scope.Show_Customers_Clipboard = function () {
        $scope.Customers_Clipboard_Listed = [];
        $scope.Customers_Clipboard_Rows = $scope.Customers_Clipboard.split('\n');
        for (var i = 0; i < $scope.Customers_Clipboard_Rows.length; i++) {
            $scope.Customers_Clipboard_Cell = $scope.Customers_Clipboard_Rows[i].split('\t');
            $scope.Customers_Clipboard_Listed.push({
                fname: $scope.Customers_Clipboard_Cell[0].trim(),
                lname: $scope.Customers_Clipboard_Cell[1].trim(),
                ncode: $scope.Customers_Clipboard_Cell[2].trim(),
                gender: $scope.Customers_Clipboard_Cell[3].trim(),
                phone: $scope.Customers_Clipboard_Cell[4].trim()
            });
        }
        $scope.Customers_Clipboard = "دریافت شد";
    };//دریافت اطلاعات از کلیپ بورد و تبدیل به جدول
    $scope.Add_Customer_Range = function () {
        $http({
            method: "POST",
            data: $scope.Customers_Clipboard_Listed,
            url: webaddress + "/manage/Add_Customer_Range"
        }).then(function mySuccess(response) {
            if (response.data[0] === "با موفیت انجام شد") {
                showmsg(response.data[0], "alert-success");
            } else {
                var msg = "";
                for (var i = 0; i < response.data.length; i++) {
                    msg += response.data[i];
                }
                showmsg(msg, "alert-danger");
            }
            $scope.Get_Customers();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });
    };//افزودن انبوه مشتریان
    $scope.Get_Customers = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Customers"
        }).then(function mySuccess(response) {
            $scope.customerlist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست مشتریان
    $scope.Add_Customer = function () {
        var gg;
        if ($scope.gender === "زن")
            gg = 1;
        else
            gg = 0;
        $http({
            method: "GET",
            url: webaddress + "/manage/Add_Customer?first_name=" + $scope.first_name + "&last_name=" + $scope.last_name + "&gender=" + gg + "&phone_number=" + $scope.phone_number + "&national_code=" + $scope.national_code + "&pass_word=" + $scope.pass_word
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Customers();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });
    };//افزودن مشتری جدید

    //_______________________________________________________________________________________________________________| .:: توابع سفارشات ::.

    $scope.Get_Orders = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Orders"
        }).then(function mySuccess(response) {
            $scope.orderlist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست سفارشات
    $scope.Add_Order = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Add_Order?tourid=" + $scope.selectedtour.originalObject.id + "&userid=" + $scope.selectedcustomer.originalObject.Id + "&count=" + $scope.order_count + "&trackid=" + $scope.order_trackid
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Orders();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });

        $scope.Get_Orders();
    };//افزودن سفارش جدید

    //_______________________________________________________________________________________________________________| .:: توابع فروشگاه ::.

    $scope.Get_Providers = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Providers"
        }).then(function mySuccess(response) {
            $scope.providerslist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست فروشگاه ها
    $scope.Add_Provider = function () {
        var sgg;
        if ($scope.pgender === "زن")
            sgg = 1;
        else
            sgg = 0;
        $http({
            method: "GET",
            url: webaddress + "/manage/Add_Provider?first_name=" + $scope.pfirst_name + "&last_name=" + $scope.plast_name + "&gender=" + sgg + "&phone_number=" + $scope.pphone_number + "&national_code=" + $scope.pnational_code + "&pass_word=" + $scope.ppass_word
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Providers();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });
    };//افزودن فروشگاه جدید

    //_______________________________________________________________________________________________________________| .:: توابع تور ها ::.

    $scope.Get_Tours = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Tours"
        }).then(function mySuccess(response) {
            $scope.tourlist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست تور ها
    $scope.uploader = new FileUploader({
        queueLimit: 1,
        url: webaddress + "/manage/Upload_Img"
    });//آپلودر تصویر تور
    $scope.uploader.filters.push({
        name: 'imageFilter',
        fn: function (item /*{File|FileLikeObject}*/, options) {
            var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
            return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) !== -1;
        }
    });//فیلتر گذاری آپلودر
    $scope.uploader.onCompleteItem = function (item, response, status, headers) {

        $http({
            method: "GET",
            url: webaddress +
                "/manage/Add_Tour?title=" + $scope.tour_name +
                "&startdate=" + $scope.tour_startdate +
                "&enddate=" + $scope.tour_enddate +
                "&days=" + $scope.tour_days +
                "&nights=" + $scope.tour_nights +
                "&capacity=" + $scope.tour_capacity +
                "&startpoint=" + $scope.tour_startpoint +
                "&city=" + $scope.tour_city +
                "&price=" + $scope.tour_price +
                "&img=" + response +
                "&alt=" + $scope.tour_alt +
                "&description=" + $scope.tour_description
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Tours();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });

    };//افزودن تور بعد از آپلود تصویر
    $scope.uploader.onCompleteAll = function () {
        $scope.uploader.destroy();
        $scope.uploader = new FileUploader({
            queueLimit: 1,
            url: webaddress + "/manage/Upload_Img"
        });
    };//ساخت دوباره آپلودر
    $scope.Toggle_Tour_Status = function (id) {
        $http({
            method: "GET",
            url: webaddress + "/manage/Toggle_Tour_Status?id=" + id
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Tours();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });

        $scope.Get_Tours();
    };//فعال و غیر فعال سازی تور

    //_______________________________________________________________________________________________________________| .:: توابع فروشندگان ::.

    $scope.Get_Credit_Orders = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Seller"
        }).then(function mySuccess(response) {
            $scope.sellerlist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست فروشندگان
    $scope.Get_Sellers = function () {
        $http({
            method: "GET",
            url: webaddress + "/manage/Get_Seller"
        }).then(function mySuccess(response) {
            $scope.sellerlist = response.data;
        }, function myError(response) {
            alert(response.statusText);
        });
    };//دریافت لیست فروشندگان
    $scope.Add_Seller = function () {
        var sgg;
        if ($scope.sgender === "زن")
            sgg = 1;
        else
            sgg = 0;
        $http({
            method: "GET",
            url: webaddress + "/manage/Add_Seller?first_name=" + $scope.sfirst_name + "&last_name=" + $scope.slast_name + "&gender=" + sgg + "&phone_number=" + $scope.sphone_number + "&national_code=" + $scope.snational_code + "&pass_word=" + $scope.spass_word
        }).then(function mySuccess(response) {
            showmsg(response.data, "alert-success");
            $scope.Get_Sellers();
        }, function myError(response) {
            showmsg(response.statusText, "alert-danger");
        });
    };//افزودن فروشنده جدید

    //_______________________________________________________________________________________________________________| .:: پر کردن اولیه اطلاعات ::.
    
});