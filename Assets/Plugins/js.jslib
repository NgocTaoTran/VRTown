mergeInto(LibraryManager.library, {
  obtainAccessToken: function (obj, func) {
		var returnStr = window.mollyverse_obtainAcessToken(
      UTF8ToString(obj),
      UTF8ToString(func)
    );
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
  }
});
