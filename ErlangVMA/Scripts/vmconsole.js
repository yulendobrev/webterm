var nodeId, cursor;

String.prototype.removeLastChar = function () {
    return this.slice(0, this.length - 1);
}

String.prototype.skip = function (n) {
    return this.slice(n, this.length);
}

function initNewLine() {
    var line = document.createElement("div"),
	    lineContents = "";

	for (var i = 0; i < 80; ++i) {
		lineContents += " ";
	}

    line.textContent = lineContents;
    return line;
}

function resizeToFit(element) {
	var $element = $(element);
	$element.width(element.scrollWidth + "px");
	$element.height(element.scrollHeight + "px");
}

var specialKeys = {
	13: [13, 10],
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
		if (oldPosition && oldPosition.Row != cursorPosition.Row) {
			var oldRow = getRow(oldPosition.Row);
			oldRow.innerHTML = oldRow.textContent.slice(0, oldPosition.Column) + cursor.textContent + oldRow.textContent.slice(oldPosition.Column + 1, 80);
		}
	
		var row = getRow(cursorPosition.Row);
		var char = row.textContent[cursorPosition.Column];
		cursor.textContent = char;
	
		row.innerHTML = row.textContent.slice(0, cursorPosition.Column) + cursor.outerHTML + row.textContent.slice(cursorPosition.Column + 1, 80);
		
		resetBlinkingCursor();
		oldPosition = { Row: cursorPosition.Row, Column: cursorPosition.Column };
	}
})();

(function () {
	var blinkTimerId;

	resetBlinkingCursor = function () {
    	stopBlinkingCursor();
    	blinkTimerId = setInterval(function () { $("#cursor").toggleClass("cursor") }, 500);
	}

	stopBlinkingCursor = function () {
		if (blinkTimerId) {
			clearInterval(blinkTimerId);
		}
	
		cursor = document.getElementById("cursor");
		
		if (!$(cursor).hasClass("cursor")) {
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
        
        cursor = document.createElement("span");
        cursor.id = "cursor";
        $(cursor).addClass("cursor");
        
        updateCursorPosition({Row: 0, Column: 0});
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
        		data = screenData.Data.join("");
        	
        	for (var row = screenData.Y; row < screenData.Y + screenData.Height; ++row) {
        		var rowDiv = getRow(row),
        			currentContents = rowDiv.textContent;
        			
        		var newContent = data.slice(k, k + screenData.Width);
        		rowDiv.textContent = currentContents.slice(0, screenData.X) + newContent + currentContents.slice(screenData.X + screenData.Width, 80);
        			
        		k += screenData.Width;
        	}
        	
        	updateCursorPosition(screenData.CursorPosition);
    	}
    	
    	setupUi();
    	
    	$.connection.hub.start().done(function () {
    		vm.server.startNewNode().done(function (result) {
    			nodeId = result;
    		})
    	})
    })
})();
