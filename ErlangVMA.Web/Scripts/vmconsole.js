var virtualMachineId,
    size = {
        columns: 80,
        rows: 24
    };

String.prototype.removeLastChar = function () {
    return this.slice(0, this.length - 1);
}

String.prototype.skip = function (n) {
    return this.slice(n, this.length);
}

function initNewLine() {
    var line = document.createElement("div"),
        cell;

    for (var i = 0; i < size.columns; ++i) {
        cell = document.createElement("span");
        cell.textContent = ' ';
        line.appendChild(cell);
    }

    return line;
}

function resizeToFit(vmConsoleDisplay) {
    $(vmConsoleDisplay).width($(vmConsoleDisplay.children[0].children[0]).width() * size.columns);
}

var specialKeys = {
    16: [],
    33: [27, 91, 53, 126],
    34: [27, 91, 54, 126],
    35: [27, 79, 70],
    36: [27, 79, 72],
    37: [27, 91, 68],
    38: [27, 91, 65],
    39: [27, 91, 67],
    40: [27, 91, 66],
    45: [27, 91, 50, 126],
    46: [27, 91, 51, 126]
};

(function () {
    function setupUi() {
        var vmConsole = document.getElementById("vmConsole"),
            vmConsoleArea = document.getElementById("vmConsoleArea"),
            vmConsoleDisplay = document.getElementById("vmConsoleDisplay"),
            refresh = document.getElementById("refresh");

        getRow = function (n) {
            return vmConsoleDisplay.children[n];
        };

        (function () {
            var blinkTimerId, cursor, $cursor;

            updateCursorPosition = function (cursorPosition) {
                if ($cursor) {
                    $cursor.removeClass("cursor");
                }

                var row = getRow(cursorPosition.r);
                cursor = row.children[cursorPosition.c];
                $cursor = $(cursor);

                $cursor.addClass("cursor");

                resetBlinkingCursor();
            }

            resetBlinkingCursor = function () {
                stopBlinkingCursor();
                blinkTimerId = setInterval(function () { $cursor.toggleClass("cursor"); }, 500);
            }

            stopBlinkingCursor = function () {
                if (blinkTimerId) {
                    clearInterval(blinkTimerId);
                }

                $cursor.addClass("cursor");
            }
        })();

        for (var i = 0; i < size.rows; ++i) {
            var line = initNewLine();
            vmConsoleDisplay.appendChild(line);
        }

        updateCursorPosition({ r: 0, c: 0 });

        resizeToFit(vmConsoleDisplay);
        $(vmConsole).width($(vmConsoleDisplay).width());
        
        function handleConsoleKeyDown(e) {
            var vmServer = $.connection.virtualMachineHub.server;

            resetBlinkingCursor();

            console.log("Key down code: " + e.keyCode);

            if (e.ctrlKey) {
                if (e.keyCode >= 64 && e.keyCode < 96) {
                    vmServer.processInput(virtualMachineId, e.keyCode - 64);
                }
                e.preventDefault();
            } else if (e.keyCode in specialKeys) {
                vmServer.processChunkInput(virtualMachineId, specialKeys[e.keyCode]);
                e.preventDefault();
            } else if (!e.shiftKey && e.keyCode <= 32) {
                vmServer.processInput(virtualMachineId, e.keyCode);
                e.preventDefault();
            }
        }

        $(vmConsoleArea).keydown(handleConsoleKeyDown);
        $(vmConsoleArea).keyup(function (e) {
            console.log("Key " + e.which + " up");
        });
        $(vmConsoleArea).keypress(function (e) {
            console.log("Key " + e.charCode + " pressed");

            if (e.charCode !== 0 && !e.ctrlKey) {
                $.connection.virtualMachineHub.server.processInput(virtualMachineId, e.charCode);
            }

            e.preventDefault();
        });
        
        $(vmConsole).click(function () { vmConsoleArea.focus() });
        
        $(vmConsole).focusout(stopBlinkingCursor);
        $(vmConsole).focusin(resetBlinkingCursor);
        
        $(vmConsoleArea).focus();
        
        $(refresh).click(function () {
            refresh.disabled = true;
        
            var vm = $.connection.virtualMachineHub,
                screenRefresh = vm.server.getScreen(virtualMachineId);
            
            screenRefresh.done(function (screen) {
                vm.client.updateScreen(virtualMachineId, screen);
            }).fail(function (error) {
                alert("Refresh failed: " + error);
            }).always(function () {
                refresh.disabled = false;
            });
        });
    }
    
    $(function () {
        var vm = $.connection.virtualMachineHub;

        var colors = ["black", "white", "red", "yellow", "green", "cyan", "blue", "magenta"];

        vm.client.updateScreen = function (id, screenUpdate) {
            for (var j = 0; j < screenUpdate.u.length; ++j) {
                var k = 0,
                    screenData = screenUpdate.u[j];

                for (var row = screenData.y; row < screenData.y + screenData.h; ++row) {
                    var rowDiv = getRow(row);

                    var newContent = screenData.d;
                    for (var i = k; i < k + screenData.w; ++i) {
                        var cell = rowDiv.children[screenData.x + i - k];
                        cell.textContent = newContent[i].c;

                        if (newContent[i].r.f != 1) {
                            cell.style.color = colors[newContent[i].r.f];
                        } else {
                            cell.style.color = "";
                        }
                        if (newContent[i].r.b != 0) {
                            cell.style.backgroundColor = colors[newContent[i].r.b];
                        } else {
                            cell.style.backgroundColor = "";
                        }
                    }

                    k += screenData.w;
                }
            }

            updateCursorPosition(screenUpdate.c);
        }

        setupUi();

        $.connection.hub.logging = true;

        $.connection.hub.start().done(function () {
            var vmConsole = $("#vmConsole");

            virtualMachineId = vmConsole.data("virtualMachineId");

            if (virtualMachineId) {
                vm.server.registerForScreenUpdates(virtualMachineId);
            }
        });
    });
})();
