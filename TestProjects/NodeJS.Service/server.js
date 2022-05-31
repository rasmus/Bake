const PORT = process.env.PORT || 4321

// Require the framework and instantiate it
const fastify = require('fastify')({ logger: true })

// Declare a route
fastify.get('/', async (request, reply) => {
  return { message: 'hello world' }
})

fastify.get('/ping', async (request, reply) => {
  return { message: 'pong' }
})

// Run the server!
const start = async () => {
  try {
    await fastify.listen(PORT)
  } catch (err) {
    fastify.log.error(err)
    process.exit(1)
  }
}

async function closeGracefully(signal) {
    await fastify.close()
    process.exit()
}
process.on('SIGINT', closeGracefully)

start()
