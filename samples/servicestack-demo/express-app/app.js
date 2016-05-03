var express = require('express');
var path = require('path');
var favicon = require('serve-favicon');
var logger = require('morgan');
var cookieParser = require('cookie-parser');
var bodyParser = require('body-parser');
var httpProxy = require('http-proxy');

// import route definitions
var indexRoute = require('./routes/index');

// init express app
var app = express();

// setup the static route
app.use(express.static(path.join(__dirname, 'public')));

// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'hbs');

// uncomment after placing your favicon in /public
//app.use(favicon(path.join(__dirname, 'public', 'favicon.ico')));
app.use(logger('dev'));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(cookieParser());

// register routes
app.use('/', indexRoute);

var proxy = httpProxy.createProxyServer({});

// if the URL was not found, default to use the proxy
var targetUrl = 'http://localhost:8098';
app.use(function(req, res, next) {
  // if  you know which urls you want to proxy, you could add a condition,
  // such as: if (req.url.indexOf('/api') === 0) { /* proxy here */ } else { next(); }
  console.log('Proxying request: (' + req.method + ') ' + req.url + ' with headers ' + JSON.stringify(req.headers));
  proxy.web(req, res, { target: targetUrl, changeOrigin: true, autoRewrite: true },
      function (err, req, res) {
          var err = new Error(err.message);
          err.status = err.status || 500;
          next(err);
      });
});

app.use(function(req, res, next) {
    // Typically, you would throw a 404 error
    var err = new Error('Not Found');
    err.status = 404;
    next(err);
});

// error handlers

// development error handler
// will print stacktrace
if (app.get('env') === 'development') {
  app.use(function(err, req, res, next) {
    res.status(err.status || 500);
    res.render('error', {
      message: err.message,
      error: err
    });
  });
}

// production error handler
// no stacktraces leaked to user
app.use(function(err, req, res, next) {
  res.status(err.status || 500);
  res.render('error', {
    message: err.message,
    error: {}
  });
});

module.exports = app;
