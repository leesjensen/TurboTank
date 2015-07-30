var turboTankControllers = angular.module('turboTankControllers', ['ngCookies']);


turboTankControllers.controller('commonCtrl', function ($route, $scope, $location, $interval, $cookies, $modal, dataCenter) {
});



turboTankControllers.controller('rootCtrl', function ($scope, $routeParams, dataCenter) {

    $scope.startGame = function (gameId) {
        dataCenter.startGame(gameId);
    }
});









