// Loads "today's specials" from Bella Vista's own REST API (Controllers/Api/SpecialsController.cs)
// without a full page reload - chapter 10 "Die REST-API von Umbraco".
$(function () {
    var $list = $("#specials-list");
    var $button = $("#specials-load-more");
    var $empty = $("#specials-empty");
    var startFrom = 0;
    var take = parseInt($button.data("take"), 10) || 3;

    function renderSpecial(item) {
        var image = item.imageUrl
            ? '<img src="' + item.imageUrl + '" alt="" class="img-responsive" />'
            : "";
        var fire = "";
        for (var i = 1; i <= 3; i++) {
            fire += '<i class="fa fa-fire ' + (i <= item.spiceLevel ? "spice-active" : "spice-inactive") + '"></i>';
        }
        return (
            '<div class="col-md-3 span1_of_4"><div class="span4_of_list dish-card">' +
            image +
            "<h3>" + item.name + "</h3>" +
            '<p>' + item.description + '</p>' +
            '<div class="dish-price">' + item.price + '</div>' +
            '<div class="spice-level">' + fire + "</div>" +
            "</div></div>"
        );
    }

    function loadMore() {
        $.get("/umbraco/api/specials/getspecials?startFrom=" + startFrom + "&take=" + take, function (data) {
            data.items.forEach(function (item) {
                $list.append(renderSpecial(item));
            });
            startFrom += data.items.length;

            if (startFrom >= data.total || data.items.length === 0) {
                $button.hide();
                $empty.show();
            }
        });
    }

    if ($list.length) {
        loadMore();
        $button.on("click", loadMore);
    }
});
