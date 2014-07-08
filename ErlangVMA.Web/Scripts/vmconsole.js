var nodeId, cursor;

String.prototype.removeLastChar = function () {
    return this.slice(0, this.length - 1);
}

String.prototype.skip = function (n) {
    return this.slice(n, this.length);
}

function initNewLine() {
    var line = document.createElement("div"),
        cell;

    for (var i = 0; i < 80; ++i) {
        cell = document.createElement("span");
        cell.textContent = ' ';
        line.appendChild(cell);
    }

    return line;
}

function resizeToFit(element) {
    var $element = $(element);
    $element.width(element.scrollWidth + "px");
    $element.height(element.scrollHeight + "px");
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

function handleConsoleKeyDown(e) {
    var vmConsoleArea = document.getElementById("vmConsoleArea"),
        vmConsoleDisplay = document.getElementById("vmConsoleDisplay"),
        vmServer = $.connection.virtualMachineHub.server;

    resetBlinkingCursor();

    console.log("Key down code: " + e.keyCode);

    if (e.ctrlKey) {
        if (e.keyCode >= 64 && e.keyCode < 96) {
            vmServer.processInput(nodeId, e.keyCode - 64);
        }
        e.preventDefault();
    } else if (e.keyCode in specialKeys) {
        vmServer.processChunkInput(nodeId, specialKeys[e.keyCode]);
        e.preventDefault();
    } else if (!e.shiftKey && e.keyCode <= 32) {
        vmServer.processInput(nodeId, e.keyCode);
        e.preventDefault();
    }
}

function getRow(n) {
    var vmConsoleDisplay = document.getElementById("vmConsoleDisplay");
    return vmConsoleDisplay.children[n];
}

(function() {
    var oldPosition;

    updateCursorPosition = function (cursorPosition) {
        $(cursor).removeClass("cursor");

        var row = getRow(cursorPosition.r);
        cursor = row.children[cursorPosition.c];

        $(cursor).addClass("cursor");

        resetBlinkingCursor();
        oldPosition = { r: cursorPosition.r, c: cursorPosition.c };
    }
})();

(function () {
    var blinkTimerId;

    resetBlinkingCursor = function () {
        stopBlinkingCursor();
        blinkTimerId = setInterval(function () { if (cursor) { $(cursor).toggleClass("cursor"); } }, 500);
    }

    stopBlinkingCursor = function () {
        if (blinkTimerId) {
            clearInterval(blinkTimerId);
        }
    
        if (cursor && $(cursor).hasClass("cursor")) {
            $(cursor).addClass("cursor");
        }
    }
})();

(function () {
    function setupUi() {
        var vmConsole = document.getElementById("vmConsole"),
            vmConsoleArea = document.getElementById("vmConsoleArea"),
            vmConsoleDisplay = document.getElementById("vmConsoleDisplay"),
            refresh = document.getElementById("refresh");

        for (var i = 0; i < 25; ++i) {
            var line = initNewLine();
            vmConsoleDisplay.appendChild(line);
        }

        updateCursorPosition({r: 0, c: 0});
        resizeToFit(vmConsoleDisplay);
        
        $(vmConsoleArea).keydown(handleConsoleKeyDown)
        $(vmConsoleArea).keyup(function (e) {
            console.log("Key " + e.which + " up");
        })
        $(vmConsoleArea).keypress(function (e) {
            console.log("Key " + e.charCode + " pressed");

            if (e.charCode !== 0 && !e.ctrlKey) {
                $.connection.virtualMachineHub.server.processInput(nodeId, e.charCode);
            }
            
            e.preventDefault();
        })
        
        $(vmConsole).click(function () { vmConsoleArea.focus() });
        
        $(vmConsole).focusout(stopBlinkingCursor);
        $(vmConsole).focusin(resetBlinkingCursor);
        
        $(vmConsoleArea).focus();
        
        $(refresh).click(function () {
            refresh.disabled = true;
        
            var vm = $.connection.virtualMachineHub,
                screenRefresh = vm.server.getScreen(nodeId);
            
            screenRefresh.done(function (screen) {
                vm.client.updateScreen(nodeId, screen);
            }).fail(function (error) {
                alert("Refresh failed: " + error);
            }).always(function () {
                refresh.disabled = false;
            });
        });
    }
    
    $(function () {
        var vm = $.connection.virtualMachineHub;
        
        vm.client.updateScreen = function (id, screenData) {
            var k = 0,
                data = screenData.d;
            
            for (var row = screenData.y; row < screenData.y + screenData.h; ++row) {
                var rowDiv = getRow(row);
                    
                var newContent = data.slice(k, k + screenData.w);
                for (var i = 0; i < newContent.length; ++i) {
                    var cell = rowDiv.children[screenData.x + i];

                    cell.textContent = newContent[i].c;

                    var colors = ["black", "white", "red", "yellow", "green", "cyan", "blue", "magenta"];
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
            
            updateCursorPosition(screenData.c);
        }
        
        setupUi();
        
        $.connection.hub.start().done(function () {
            vm.server.startNewNode().done(function (result) {
                nodeId = result;
            })
        })
    })
})();
