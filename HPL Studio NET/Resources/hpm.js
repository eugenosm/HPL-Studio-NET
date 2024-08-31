Prism.languages['hpm'] = {
	'comment': {
		pattern: /;.*/,
		greedy: true
	},
	'string': {
		pattern: /(["'])(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,
		greedy: true
	},
	'char': {
		pattern: /'(?:[^'\r\n]{0,4}|'')'/,
		greedy: true
	},
	'class-name': /(?<=\s?=\s?)(E|S|T|L|P|A)(?=\()|(?<=R[0-9A-Fa-f]{1,2}[\s]?\=[\s]?[\w\s]+\,)(B|C|D|E|H\d*|L|S|P)/,
	'register': {
		pattern: /\$AREA\b|\$BLOCKSIZE\b|\$CDELAY\b|\$CHNS\b|\$FREQ\b|\$MAXSIZE\b|\$SIZE\b|\$VERIFY\b|\$VERSION\b|\$WDELAY\b|\$MODE\b|\$MCUCLK|\b(R1[0-9a-fA-F]|R[0-9a-fA-F]|VCC|VPP)\b|@\w+/,
		alias: 'symbol'
	},
	'instruction': {
		pattern: /\b(ADR|ALLPINS|ALTNAME|BCOLOR|BGCOLOR|BREAK|BUSI|BUSO|CDELAY|CFREQ|CONST|DATA|EXIT|GAP|GET|HPL|IDISABLE|IENABLE|IMAGE|INFO|INFOFILE|LOOP|MARK|MATRIX|OPTIONS|PIN[GIO]?|PLACE|POWER|PRINT|REG|RETURN|SHAPE|SIZE|SOCKET|SPACE|TCOLOR|T[DLRU])\b/,
		alias: 'keyword'
	},
	'directive': {
		pattern: /(\#v|\#include|\#def|\#struct|\#ends|\#macro|\#endm|\$OPERATION(\.READ|\.VERIFY|\.WRITE|\.USER)?|\$VALUE(\.Ok|\.Cancel)?|#type\.(bin|check_box|dec|hex_editor|hex|list|string|push_button)?)\b/,
		alias: 'property'
	},
	'hex-number': {
		pattern: /\b(0x[\da-fA-f]{1,16}\b|0x\d[\da-fA-f]{0,15}\b)/i,
		alias: 'number'
	},
	'binary-number': {
		pattern: /\b[01]+[Bb]\b/,
		alias: 'number'
	},
	'decimal-number': {
		pattern: /\b\d+\b/,
		alias: 'number'
	},
	'operator': /[<>]=?|[!=]=?=?|--?|\+\+?|&&?|\|\|?|[?*/~^%]/,
	'punctuation': /[{}[\];(),.:]/

};