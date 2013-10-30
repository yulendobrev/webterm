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

var specialKeys = {
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
    }
}

function getRow(n) {
	var vmConsoleDisplay = document.getElementById("vmConsoleDisplay");
	return vmConsoleDisplay.children[n];
}

function updateCursorPosition(cursorPosition) {
	var row = getRow(cursorPosition.Row);
	var char = row.textContent[cursorPosition.Column];
	cursor.textContent = char;
	
	row.innerHTML = row.textContent.slice(0, cursorPosition.Column) + cursor.outerHTML + row.textContent.slice(cursorPosition.Column + 1, 80)
}

(function () {
	var blinkTimerId;

	resetBlinkingCursor = function () {
    	stopBlinkingCursor();
    	blinkTimerId = setInterval(function () { $(cursor).toggleClass("cursor") }, 500);
	}

	stopBlinkingCursor = function () {
		if (blinkTimerId) {
			clearInterval(blinkTimerId);
		}
	
		if (!$(cursor).hasClass("cursor")) {
        	$(cursor).addClass("cursor");
        }
	}
})();

(function () {
    function setupUi() {
        var vmConsole = document.getElementById("vmConsole"),
            vmConsoleArea = document.getElementById("vmConsoleArea"),
            vmConsoleDisplay = document.getElementById("vmConsoleDisplay");

		for (var i = 0; i < 25; ++i) {
        	var line = initNewLine();
        	vmConsoleDisplay.appendChild(line);
        }
        
        cursor = document.createElement("span");
        $(cursor).addClass("cursor");
        
        updateCursorPosition({Row: 0, Column: 0});
        
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
    }
    
    $(function () {
    	var vm = $.connection.virtualMachineHub;
    	
    	vm.client.updateScreen = function (id, screenData) {
        	console.log("Received " + screenData.Data);
        	
        	var k = 0,
        		data = screenData.Data.join("");
        	
        	for (var row = screenData.Y; row < screenData.Y + screenData.Width; ++row) {
        		var rowDiv = getRow(row),
        			currentContents = rowDiv.textContent;
        			
        		var newContent = data.slice(k, k + screenData.Height);
        		rowDiv.textContent = currentContents.slice(0, screenData.X) + newContent + currentContents.slice(screenData.X + screenData.Height, 80);
        			
        		k += screenData.Height;
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
