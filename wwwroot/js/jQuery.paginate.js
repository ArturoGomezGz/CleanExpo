/**
 *
 * @summary     jPaginate
 * @description Paginate an html elements
 * @version     2017.03.09
 * Rrepository  https://github.com/asirokas/jPaginate
 *
 * @author      Athanasios Sirokas (www.asirokas.com)
 * Contact      asirokas@gmail.com
 *
 */
(function ($) {
    $.fn.paginate = function (options) {
        var defaults = {
            pagination_class: "pagination",
            items_per_page: 5,
            prev_next: true,
            prev_text: '&laquo;',
            next_text: '&raquo;',
            numPedido: 0,
            paginationPages: 5,
            currentPage: 1
        };

        // Merge deafults into options
        var options = $.extend(defaults, options);

        obj = $(this);

        // Count entries in block for pagination
        var hijos = obj.children();

        var n = Array.from(hijos).filter(function (hijo) {
            return window.getComputedStyle(hijo).display !== 'none';
        }).length;

        // Calculate number of pages
        var pages = Math.ceil(n / options.items_per_page);

        // Add a div after the #slideshow to put the navigation controls in
        obj.append(
            $('<div/>').addClass("pagination__controls").append(
                $('<ul/>').addClass(options.pagination_class))
        );

        function createPaginationControls(pages) {

            // Add Previous button
            if (options.prev_next == true) {
                obj.find("." + options.pagination_class).append('<li class="prev"><a href="#">' + options.prev_text + '</a></li>');
            }

            // For each div (slide) add a link in span for controls
            for (var i = 0; i < pages; i++) {
                obj.find("." + options.pagination_class).append('<li class="page"><a href="#' + (i + 1) + '">' + (i + 1) + '</a></li>');
            };

            // Add Next Button
            if (options.prev_next == true) {
                obj.find("." + options.pagination_class).append('<li class="next"><a href="#">' + options.next_text + '</a></li>');
            }
        }

        function showPagePagination(page) {
            var totalPages = $('.' + options.pagination_class +' .page').length;
            currentPage = page;
            $('.' + options.pagination_class +' .page').addClass('d-none');
            var start = (currentPage - 1) - ((currentPage - 1) % options.paginationPages);
            var end = Math.min(start + options.paginationPages, totalPages);

            for (var i = start; i < end; i++) {
                $('.' + options.pagination_class +' .page').eq(i).removeClass('d-none');
            }

            $('.' + options.pagination_class +' .page a').removeClass('active');
            $('.' + options.pagination_class +' .page a').eq(currentPage - 1).addClass('active');

            if (currentPage === 1) {
                $('.' + options.pagination_class +' .prev').addClass('d-none');
            } else {
                $('.' + options.pagination_class +' .prev').removeClass('d-none');
            }

            if (currentPage === totalPages) {
                $('.' + options.pagination_class +' .next').addClass('d-none');
            } else {
                $('.' + options.pagination_class +' .next').removeClass('d-none');
            }
        }

        $('.' + options.pagination_class).on('click', '.page', function (e) {
            e.preventDefault();
            var page = parseInt($(this).text());
            showPagePagination(page);
        });

        $('.' + options.pagination_class).on('click', '.prev', function (e) {
            e.preventDefault();
            if (currentPage > 1) {
                showPagePagination(currentPage - 1);
            }
        });

        $('.' + options.pagination_class).on('click', '.next', function (e) {
            e.preventDefault();
            var totalPages = $('.' + options.pagination_class + ' .page').length;
            if (currentPage < totalPages) {
                showPagePagination(currentPage + 1);
            }
        });

        function showPage(page_number) {
            var start_from = (page_number * options.items_per_page);
            var end_on = ((page_number + 1) * options.items_per_page);
            var displayDivs
            if (options.numPedido == 0) {
                displayDivs = obj.children();
            } else {
                displayDivs = obj.children('#Pedido_' + options.numPedido);
            }

            displayDivs.not('.pagination__controls').css('display', 'none').slice(start_from, end_on).css('display', 'block');
        }

        createPaginationControls(pages);
        showPage(0);
        showPagePagination(1);

        obj.find('.' + options.pagination_class + ' li').not(".prev,.next").first().addClass('active');

        // Navigate to the coresponding slide when clicking on a nav-control
        obj.find('.' + options.pagination_class + ' li').not(".prev,.next").click(function () {

            // Reset the obj element to match the current pagination element
            obj = $(this).parent().parent().parent();

            if (options.prev_next == true) {
                var pageIndex = $(this).index() - 1;
            } else {
                var pageIndex = $(this).index();
            };

            // remove active class from all elements
            $(this).parent().children().removeClass('active');
            $(this).addClass('active');
            showPage(pageIndex);
        });

        // Navigate to the previous slide when clicking on the prev button
        obj.find('.' + options.pagination_class + ' li.prev').click(function () {

            // Reset the obj element to match the current pagination element
            obj = $(this).parent().parent().parent();

            pageIndex = $(this).parent().find('li.active').index() - 2;
            if (pageIndex < 0) pageIndex = 0;

            $(this).parent().find('li.active').removeClass('active');
            $(this).parent().find('li:nth-child(' + (pageIndex + 2) + ')').addClass('active');
            showPage(pageIndex);
        });

        // Navigate to the next slide when clicking on the next button
        obj.find('.' + options.pagination_class + ' li.next').click(function () {

            // Reset the obj element to match the current pagination element
            obj = $(this).parent().parent().parent();

            pageIndex = $(this).parent().find('li.active').index();
            if (pageIndex > pages - 1) pageIndex = pages - 1;

            $(this).parent().find('li.active').removeClass('active');
            $(this).parent().find('li:nth-child(' + (pageIndex + 2) + ')').addClass('active');
            showPage(pageIndex);
        });

    };
})(jQuery);
