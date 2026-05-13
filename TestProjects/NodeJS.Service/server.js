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
    const port = Number.parseInt(PORT, 10)

    if (Number.isNaN(port)) {
      throw new Error(`Invalid PORT value: ${PORT}`)
    }

    await fastify.listen({ port, host: "0.0.0.0" })
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
