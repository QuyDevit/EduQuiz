$(document).ready(function () {
    function reloadPartialFolders() {
        $.ajax({
            url: '/Library/_PartialFolders',
            type: 'GET',
            success: function (result) {
                $('#partialfolder-container').html(result);
                updateActiveClasses();
            },
            error: function (xhr, status, error) {
                console.error('AJAX error:', status, error);
            }
        });
    }
    $("#savename").click(function () {
        var id = $("#savename").attr("data-id");
        var name = $("#editname").val()
        $.ajax({
            type: "POST",
            url: "/Library/RenameEduQuiz",
            data: {
                name: name,
                idquiz: id
            },
            success: function (response) {
                if (response.result == "PASS") {
                    $(`#title-${id}`).text(name);
                    $("#close").click();
                }
            },
            error: function (err) {

            }
        })
    })
    $(".styles__Button-menu.duplicate").click(function () {
        Swal.fire({
            title: "Bạn có muốn nhân bản EduQuiz ngay tại đây",
            showCancelButton: true,
            confirmButtonText: "Nhân bản",
            CancelButtonText:"Hủy"
        }).then((result) => {
            if (result.isConfirmed) {
                var idquiz = $(this).attr("data-quesion");
                var idfolder = $(this).attr("data-folder");
                $.ajax({
                    type: "POST",
                    url: "/Library/DuplicateEduQuiz",
                    data: {
                        idquiz: idquiz,
                        folderid: idfolder
                    },
                    success: function (response) {
                        if (response.result == "PASS") {
                            location.reload();
                        }
                    },
                    error: function (err) {

                    }
                })
            } 
        });
        
    })
   
    $('#partialfolder-container').on('keydown', '.input__add-folder', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            var folderName = $(this).val().trim();
            var idfolderRoot = $(this).attr("data-id");
            if (folderName) {
                $.ajax({
                    url: '/Library/AddFolder',
                    type: 'POST',
                    data: {
                        folderName: folderName,
                        idroot: idfolderRoot
                    },
                    success: function (result) {
                        reloadPartialFolders();
                    },
                    error: function (xhr, status, error) {
                        console.error('AJAX error:', status, error);
                    }
                });
            }
        }
    });
    function updateActiveClasses() {
        $('.sidebar-Link__Wrapper a').each(function () {
            var href = $(this).attr('href');
            if (href === urlcurrent) {
                $(this).addClass('active');
            } else {
                $(this).removeClass('active');
            }
        });
        // Cập nhật lớp active cho sidebar folders root
        var $root = $('.sidebar-folders__Root');
        var rootHref = $root.find('a').attr('href');
        if (rootHref === urlcurrent) {
            $root.addClass('active');
        } else {
            $root.removeClass('active');
        }
    }
    $(document).on('click', "#savefolder", function () {
        var flagcheck = $(this).attr("data-flag");
        var folderId = $(this).attr("data-id");
        var folderIdCurrent = 0;
        if (typeof foldercurrent !== 'undefined' && foldercurrent !== null) {
            folderIdCurrent = foldercurrent;
        }
        if (!flagcheck) {
            var eduQuizId = $(this).attr("data-eduquiz");
            if (!folderId || !eduQuizId) {
                openToast('warning', 'Lỗi', "Vui lòng chọn EduQuiz và thư mục cần di chuyển!", 2500);
                return;
            }
            $.ajax({
                url: '/Library/MoveEduQuiz',
                type: 'POST',
                data: {
                    idfolder: folderId,
                    ideduquiz: eduQuizId,
                    idfoldercurrent: folderIdCurrent
                },
                success: function (response) {
                    if (response.result === "PASS") {
                        location.href = `/my-library/eduquizs/${response.data}`;
                    } else {
                        location.href = `/my-library/eduquizs/${response.data}`;
                    }
                },
                error: function (xhr, status, error) {
                    console.error('AJAX error:', status, error);
                }
            });
        } else {
            if (!folderId || selectedCheckboxes.length === 0) {
                openToast('warning', 'Lỗi', "Vui lòng chọn EduQuiz và thư mục cần di chuyển!", 2500);
                return;
            }
            $.ajax({
                url: '/Library/MoveMutiEduQuiz',
                type: 'POST',
                data: {
                    idfolder: folderId,
                    ideduquiz: selectedCheckboxes,
                    idfoldercurrent: folderIdCurrent
                },
                success: function (response) {
                    if (response.result === "PASS") {
                        location.href = `/my-library/eduquizs/${response.data}`;
                    } else {
                        location.href = `/my-library/eduquizs/${response.data}`;
                    }
                },
                error: function (xhr, status, error) {
                    console.error('AJAX error:', status, error);
                }
            });
        }
        
    })
    $(document).on('click', '.styles__Button.p-2', function (event) {
        event.stopPropagation();
        var menuId = $(this).attr('id').replace('openmenu-', '#menu-');
        var $menu = $(menuId);
        $('.list-action__menu').not($menu).addClass('d-none');
        $menu.toggleClass('d-none');
    });
    $(document).on('click', '.item-folder', function (event) {
        event.stopPropagation();
        var menuId = $(this).attr('id').replace('openfolder-', '#menufolder-');
        var $menu = $(menuId);
        $('.menu-folder').not($menu).addClass('d-none');
        $menu.toggleClass('d-none');
    });

    $(document).on('click', function () {
        $('.list-action__menu').addClass('d-none');
        $('.menu-folder').addClass('d-none');
    });

    $(document).on('click', '.list-action__menu, .menu-folder', function (event) {
        event.stopPropagation();
    });

    $(document).on('click', '.add-folders', function () {
        let check = $("#addfolder-wrapper").hasClass("d-none");
        var inputfolder = $(".input__add-folder");
        $("#addfolder-wrapper").toggleClass("d-none");
        if (check) {
            inputfolder.focus();
        } else {
            inputfolder.val(""); // Clear input if the wrapper is hidden
        }
    });

    $(document).on('click', '.sidebar-Link__Wrapper a', function (event) {
        if ($(this).hasClass("active")) {
            event.preventDefault();
        }
    });

    $(document).on('click', '.sidebar-folders__Root a', function (event) {
        if ($(this).closest(".sidebar-folders__Root").hasClass("active")) {
            event.preventDefault();
        }
    });   

});
