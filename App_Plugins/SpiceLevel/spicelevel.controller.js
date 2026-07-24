// Custom property editor (chapter 6, "Eigene Property Editoren schreiben"): a simple
// 0-3 chili-icon picker for how spicy a dish is, following the same
// package.manifest + AngularJS controller pattern taught in class.
angular.module("umbraco").controller("BellaVista.SpiceLevelController", function ($scope) {
    if (!$scope.model.value && $scope.model.value !== 0) {
        $scope.model.value = 0;
    }

    $scope.setLevel = function (level) {
        // clicking the currently selected chili clears it back to 0
        $scope.model.value = $scope.model.value === level ? 0 : level;
    };
});
