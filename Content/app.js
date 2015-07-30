var zeusApp = angular.module('zeusApp', [
'ngRoute',
'turboTankControllers',
'ui.bootstrap',
'xeditable'
]);

zeusApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

zeusApp.config(['$routeProvider',
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


zeusApp.factory('dataCenter', function ($http) {
    return {
        startGame: function (gameId) {
            return $http.post('/game/' + gameId, {});
        },
    }
});

