var turboTankApp = angular.module('turboTankApp', [
'ngRoute',
'turboTankControllers',
'ui.bootstrap',
'xeditable'
]);

turboTankApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

turboTankApp.config(['$routeProvider',
  function ($routeProvider) {
      $routeProvider.
        when('/', {
            templateUrl: 'content/view/root.html',
            controller: 'rootCtrl'
        }).
        otherwise({
            redirectTo: '/'
        });
  }]);


turboTankApp.factory('dataCenter', function ($http) {
    return {
        startGame: function (server, port, gameId) {
            return $http.post('/game/' + gameId + '?server=' + server + '&port=' + port, {});
        },
    }
});

