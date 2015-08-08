var turboTankControllers = angular.module('turboTankControllers', ['ngCookies']);


turboTankControllers.controller('commonCtrl', function ($route, $scope, $location, $interval, $cookies, $modal, dataCenter) {
});



turboTankControllers.controller('rootCtrl', function ($scope, $routeParams, dataCenter) {
    $scope.server = "127.0.0.1";
    $scope.port = "8080";
    $scope.gameId = "tankyou";

    $scope.startGame = function (server, port, gameId) {
        dataCenter.startGame(server, port, gameId);
    }
});









