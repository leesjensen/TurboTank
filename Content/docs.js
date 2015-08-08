var docsApp = angular.module('docsApp', ['ui.bootstrap']);

docsApp.controller('docsCtrl', function ($scope, $http) {
    $http.get('/docs').success(function (data) {
        $scope.docs = data;
        $scope.currentDoc = $scope.docs[0];

        $scope.myReq = JSON.stringify($scope.currentDoc.responseExample, undefined, 2);
    });

    $scope.setCurrentDoc = function (currentDoc) {
        $scope.currentDoc = currentDoc;
    };

    $scope.makeRequest = function (currentDoc) {
        var req = {
            method: currentDoc.method,
            url: currentDoc.requestExample,
            data: currentDoc.requestExampleBody,
        }

        $http(req).success(function (data, status, headers, config) {
            currentDoc.responseExample = data;
        }).error(function (data, status, headers, config) {
            currentDoc.responseExample = JSON.stringify(data, undefined, 2);
        });
    }
});

docsApp.directive('jsonText', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attr, ngModel) {
            function into(input) {
                return JSON.parse(input);
            }
            function out(data) {
                return JSON.stringify(data, undefined, 2);
            }
            ngModel.$parsers.push(into);
            ngModel.$formatters.push(out);
        }
    };
});





docsApp.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});